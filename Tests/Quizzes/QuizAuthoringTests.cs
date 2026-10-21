using Application.Common;
using Application.Features.Lessons.Errors;
using Application.Features.Quizzes.Commands.CreateQuiz;
using Application.Features.Quizzes.Commands.DeleteQuiz;
using Application.Features.Quizzes.Commands.SubmitQuizAttempt;
using Application.Features.Quizzes.Commands.UpdateQuiz;
using Application.Features.Quizzes.DTOs;
using Application.Features.Quizzes.Errors;
using Application.Features.Quizzes.Queries.GetQuiz;
using Application.Features.Quizzes.Queries.GetQuizManagement;
using Domain.Entities;
using FluentValidation;
using Tests.TestInfrastructure;
using Xunit;

namespace Tests.Quizzes;

public class QuizAuthoringTests : IClassFixture<TestDatabaseFixture>
{
    private readonly TestDatabaseFixture _fixture;

    public QuizAuthoringTests(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    private sealed record Setup(
        TestHost Host,
        ApplicationUser Teacher,
        ApplicationUser Student,
        int CourseId,
        int LessonId);

    private async Task<Setup> SetupAuthoringAsync()
    {
        var host = new TestHost(_fixture);

        var teacher = await host.CreateUserAsync(
            lastName: "Teacher");
        var student = await host.CreateUserAsync();
        var course = await host.CreateCourseAsync();
        var lesson = await host.CreateLessonAsync(course.Id, 0);

        await host.EnrollAsync(student.Id, course.Id);

        host.CurrentUser.SignIn(teacher, AppRoles.Teacher);

        return new Setup(host, teacher, student, course.Id, lesson.Id);
    }

    private static CreateQuizCommand ValidCreate(
        int lessonId,
        string title = "Chapter 1 Quiz",
        decimal passingScorePercent = 80m)
    {
        return new CreateQuizCommand
        {
            LessonId = lessonId,
            Title = title,
            PassingScorePercent = passingScorePercent,
            Questions = TwoQuestions()
        };
    }

    private static UpdateQuizCommand ValidUpdate(
        int lessonId,
        string title = "Chapter 1 Quiz",
        decimal passingScorePercent = 80m)
    {
        return new UpdateQuizCommand
        {
            LessonId = lessonId,
            Title = title,
            PassingScorePercent = passingScorePercent,
            Questions = TwoQuestions()
        };
    }

    private static List<QuizQuestionInput> TwoQuestions()
    {
        return new List<QuizQuestionInput>
        {
            new()
            {
                Text = "What is 2 + 2?",
                Options = new List<QuizOptionInput>
                {
                    new() { Text = "4", IsCorrect = true },
                    new() { Text = "5", IsCorrect = false }
                }
            },
            new()
            {
                Text = "What is 3 + 3?",
                Options = new List<QuizOptionInput>
                {
                    new() { Text = "6", IsCorrect = true },
                    new() { Text = "7", IsCorrect = false }
                }
            }
        };
    }

    private static async Task SeedAttemptAsync(
        TestHost host,
        ApplicationUser student,
        int lessonId)
    {
        var quiz = await host.GetQuizByLessonAsync(lessonId);
        var options = await host.GetQuizOptionsAsync(quiz.Id);

        var answers = options
            .GroupBy(o => o.QuestionId)
            .Select(g => new QuizAnswerInput
            {
                QuestionId = g.Key,
                OptionId = g.First(o => !o.IsCorrect).Id
            })
            .ToList();

        host.CurrentUser.SignIn(student);

        var result = await host.SendAsync(new SubmitQuizAttemptCommand
        {
            LessonId = lessonId,
            Answers = answers
        });

        Assert.True(result.Success);

        host.CurrentUser.SignIn(student, AppRoles.Student);
    }

    [Fact]
    public async Task TeacherCreatesQuiz_AnswerKeyVisibleInManagementView()
    {
        var setup = await SetupAuthoringAsync();
        using var host = setup.Host;

        var result = await host.SendAsync(ValidCreate(setup.LessonId));

        Assert.True(result.Success);
        Assert.NotNull(result.Data);

        var management = await host.SendAsync(
            new GetQuizManagementQuery(setup.LessonId));

        Assert.True(management.Success);
        Assert.NotNull(management.Data);
        Assert.Equal("Chapter 1 Quiz", management.Data.Title);
        Assert.Equal(80m, management.Data.PassingScorePercent);
        Assert.Equal(0, management.Data.AttemptCount);
        Assert.Equal(2, management.Data.Questions.Count);

        var question = management.Data.Questions[0];
        Assert.Equal("What is 2 + 2?", question.Text);
        Assert.Equal(2, question.Options.Count);
        Assert.True(question.Options.Single(o => o.Text == "4").IsCorrect);
        Assert.False(question.Options.Single(o => o.Text == "5").IsCorrect);
    }

    [Fact]
    public async Task ManagementView_ShowsAttemptCount_AfterStudentSubmits()
    {
        var setup = await SetupAuthoringAsync();
        using var host = setup.Host;

        var created = await host.SendAsync(ValidCreate(setup.LessonId));
        Assert.True(created.Success);

        await SeedAttemptAsync(host, setup.Student, setup.LessonId);

        host.CurrentUser.SignIn(setup.Teacher, AppRoles.Teacher);

        var management = await host.SendAsync(
            new GetQuizManagementQuery(setup.LessonId));

        Assert.True(management.Success);
        Assert.NotNull(management.Data);
        Assert.Equal(1, management.Data.AttemptCount);
    }

    [Fact]
    public async Task Student_CannotCreateQuiz()
    {
        var setup = await SetupAuthoringAsync();
        using var host = setup.Host;

        host.CurrentUser.SignIn(setup.Student);

        var result = await host.SendAsync(ValidCreate(setup.LessonId));

        Assert.False(result.Success);
        Assert.Equal(LessonErrors.NotAuthorized.Code, result.Error!.Code);
    }

    [Fact]
    public async Task Student_CannotOpenManagementView()
    {
        var setup = await SetupAuthoringAsync();
        using var host = setup.Host;

        host.CurrentUser.SignIn(setup.Student);

        var result = await host.SendAsync(
            new GetQuizManagementQuery(setup.LessonId));

        Assert.False(result.Success);
        Assert.Equal(LessonErrors.NotAuthorized.Code, result.Error!.Code);
    }

    [Fact]
    public async Task CreateTwice_ForSameLesson_IsRejected()
    {
        var setup = await SetupAuthoringAsync();
        using var host = setup.Host;

        var first = await host.SendAsync(ValidCreate(setup.LessonId));
        Assert.True(first.Success);

        var second = await host.SendAsync(ValidCreate(setup.LessonId));

        Assert.False(second.Success);
        Assert.Equal(
            QuizErrors.QuizAlreadyExists.Code,
            second.Error!.Code);
    }

    [Fact]
    public async Task Create_ForMissingLesson_IsRejected()
    {
        var setup = await SetupAuthoringAsync();
        using var host = setup.Host;

        var result = await host.SendAsync(ValidCreate(lessonId: 999999));

        Assert.False(result.Success);
        Assert.Equal(LessonErrors.LessonNotFound.Code, result.Error!.Code);
    }

    [Fact]
    public async Task QuestionWithTwoCorrectAnswers_ThrowsValidationException()
    {
        var setup = await SetupAuthoringAsync();
        using var host = setup.Host;

        var command = ValidCreate(setup.LessonId);
        command.Questions[0].Options[1].IsCorrect = true;

        await Assert.ThrowsAsync<ValidationException>(() =>
            host.SendAsync(command));
    }

    [Fact]
    public async Task QuestionWithSingleOption_ThrowsValidationException()
    {
        var setup = await SetupAuthoringAsync();
        using var host = setup.Host;

        var command = ValidCreate(setup.LessonId);
        command.Questions[1].Options.RemoveAt(1);

        await Assert.ThrowsAsync<ValidationException>(() =>
            host.SendAsync(command));
    }

    [Fact]
    public async Task NoQuestions_ThrowsValidationException()
    {
        var setup = await SetupAuthoringAsync();
        using var host = setup.Host;

        var command = ValidCreate(setup.LessonId);
        command.Questions.Clear();

        await Assert.ThrowsAsync<ValidationException>(() =>
            host.SendAsync(command));
    }

    [Fact]
    public async Task PassingScoreOutOfRange_ThrowsValidationException()
    {
        var setup = await SetupAuthoringAsync();
        using var host = setup.Host;

        var command = ValidCreate(setup.LessonId, passingScorePercent: 101m);

        await Assert.ThrowsAsync<ValidationException>(() =>
            host.SendAsync(command));
    }

    [Fact]
    public async Task Update_WithNoAttempts_ReplacesQuestions()
    {
        var setup = await SetupAuthoringAsync();
        using var host = setup.Host;

        var created = await host.SendAsync(ValidCreate(setup.LessonId));
        Assert.True(created.Success);

        var update = ValidUpdate(setup.LessonId);
        update.Title = "Renamed Quiz";
        update.PassingScorePercent = 60m;
        update.Questions = new List<QuizQuestionInput>
        {
            new()
            {
                Text = "New only question?",
                Options = new List<QuizOptionInput>
                {
                    new() { Text = "Yes", IsCorrect = true },
                    new() { Text = "No", IsCorrect = false },
                    new() { Text = "Maybe", IsCorrect = false }
                }
            }
        };

        var result = await host.SendAsync(update);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal("Renamed Quiz", result.Data.Title);
        Assert.Equal(60m, result.Data.PassingScorePercent);
        Assert.Single(result.Data.Questions);
        Assert.Equal(3, result.Data.Questions[0].Options.Count);

        var management = await host.SendAsync(
            new GetQuizManagementQuery(setup.LessonId));

        Assert.True(management.Success);
        Assert.NotNull(management.Data);
        Assert.Equal("Renamed Quiz", management.Data.Title);
        Assert.Single(management.Data.Questions);
        Assert.Equal(
            "New only question?",
            management.Data.Questions[0].Text);
    }

    [Fact]
    public async Task Update_WithAttempts_AllowsTitleChange_WhenQuestionsIdentical()
    {
        var setup = await SetupAuthoringAsync();
        using var host = setup.Host;

        var created = await host.SendAsync(ValidCreate(setup.LessonId));
        Assert.True(created.Success);

        await SeedAttemptAsync(host, setup.Student, setup.LessonId);

        host.CurrentUser.SignIn(setup.Teacher, AppRoles.Teacher);

        var update = ValidUpdate(setup.LessonId);
        update.Title = "Renamed After Attempts";

        var result = await host.SendAsync(update);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal("Renamed After Attempts", result.Data.Title);
        Assert.Equal(1, result.Data.AttemptCount);
        Assert.Equal(2, result.Data.Questions.Count);
    }

    [Fact]
    public async Task Update_WithAttempts_RejectsQuestionChanges()
    {
        var setup = await SetupAuthoringAsync();
        using var host = setup.Host;

        var created = await host.SendAsync(ValidCreate(setup.LessonId));
        Assert.True(created.Success);

        await SeedAttemptAsync(host, setup.Student, setup.LessonId);

        host.CurrentUser.SignIn(setup.Teacher, AppRoles.Teacher);

        var update = ValidUpdate(setup.LessonId);
        update.Questions[0].Text = "Altered question text?";

        var result = await host.SendAsync(update);

        Assert.False(result.Success);
        Assert.Equal(QuizErrors.HasAttempts.Code, result.Error!.Code);

        var management = await host.SendAsync(
            new GetQuizManagementQuery(setup.LessonId));

        Assert.True(management.Success);
        Assert.NotNull(management.Data);
        Assert.Equal("What is 2 + 2?", management.Data.Questions[0].Text);
    }

    [Fact]
    public async Task Delete_WithoutAttempts_RemovesQuiz()
    {
        var setup = await SetupAuthoringAsync();
        using var host = setup.Host;

        var created = await host.SendAsync(ValidCreate(setup.LessonId));
        Assert.True(created.Success);

        var deleted = await host.SendAsync(
            new DeleteQuizCommand(setup.LessonId));

        Assert.True(deleted.Success);

        var management = await host.SendAsync(
            new GetQuizManagementQuery(setup.LessonId));

        Assert.False(management.Success);
        Assert.Equal(QuizErrors.QuizNotFound.Code, management.Error!.Code);

        host.CurrentUser.SignIn(setup.Student);

        var studentView = await host.SendAsync(
            new GetQuizQuery(setup.LessonId));

        Assert.False(studentView.Success);
        Assert.Equal(QuizErrors.QuizNotFound.Code, studentView.Error!.Code);
    }

    [Fact]
    public async Task Delete_WithAttempts_IsBlocked()
    {
        var setup = await SetupAuthoringAsync();
        using var host = setup.Host;

        var created = await host.SendAsync(ValidCreate(setup.LessonId));
        Assert.True(created.Success);

        await SeedAttemptAsync(host, setup.Student, setup.LessonId);

        host.CurrentUser.SignIn(setup.Teacher, AppRoles.Teacher);

        var deleted = await host.SendAsync(
            new DeleteQuizCommand(setup.LessonId));

        Assert.False(deleted.Success);
        Assert.Equal(QuizErrors.HasAttempts.Code, deleted.Error!.Code);

        var management = await host.SendAsync(
            new GetQuizManagementQuery(setup.LessonId));

        Assert.True(management.Success);
        Assert.NotNull(management.Data);
        Assert.Equal(1, management.Data.AttemptCount);
    }
}
