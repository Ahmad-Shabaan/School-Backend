using System.Text.Json;
using Application.Features.Lessons.Errors;
using Application.Features.Quizzes.DTOs;
using Application.Features.Quizzes.Errors;
using Application.Features.Quizzes.Commands.SubmitQuizAttempt;
using Application.Features.Quizzes.Queries.GetQuiz;
using Application.Features.Quizzes.Queries.GetQuizAttempts;
using Domain.Entities;
using Tests.TestInfrastructure;
using Xunit;

namespace Tests.Quizzes;

public class QuizTests : IClassFixture<TestDatabaseFixture>
{
    private readonly TestDatabaseFixture _fixture;

    public QuizTests(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    private sealed record Setup(
        TestHost Host,
        ApplicationUser Student,
        int CourseId,
        int FirstLessonId,
        int SecondLessonId);

    private async Task<Setup> SetupQuizAsync(
        decimal passingScorePercent = 80m,
        int questionCount = 4,
        bool unlockSecondLesson = true)
    {
        var host = new TestHost(_fixture);

        var student = await host.CreateUserAsync();
        var course = await host.CreateCourseAsync();
        var lesson1 = await host.CreateLessonAsync(course.Id, 0);
        var lesson2 = await host.CreateLessonAsync(course.Id, 1);

        await host.EnrollAsync(student.Id, course.Id);
        await host.CreateQuizAsync(
            lesson1.Id, questionCount, passingScorePercent);

        if (unlockSecondLesson)
        {
            await host.ExecuteAsync(async context =>
            {
                context.LessonProgresses.Add(new Domain.Entities.LessonProgress
                {
                    ApplicationUserId = student.Id,
                    LessonId = lesson1.Id,
                    LastPositionSeconds = 100,
                    DurationSeconds = 100,
                    WatchedPercentage = 100m,
                    IsWatched = true,
                    WatchedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });

                await context.SaveChangesAsync();
            });
        }

        host.CurrentUser.SignIn(student);

        return new Setup(host, student, course.Id, lesson1.Id, lesson2.Id);
    }

    private static async Task<List<(int QuestionId, int CorrectOptionId, int WrongOptionId)>>
        GetAnswerKeyAsync(TestHost host, int quizId)
    {
        var options = await host.GetQuizOptionsAsync(quizId);

        return options
            .GroupBy(o => o.QuestionId)
            .Select(g => (
                QuestionId: g.Key,
                CorrectOptionId: g.First(o => o.IsCorrect).Id,
                WrongOptionId: g.First(o => !o.IsCorrect).Id))
            .OrderBy(x => x.QuestionId)
            .ToList();
    }

    private static List<QuizAnswerInput> AllCorrect(
        List<(int QuestionId, int CorrectOptionId, int WrongOptionId)> key)
    {
        return key
            .Select(x => new QuizAnswerInput
            {
                QuestionId = x.QuestionId,
                OptionId = x.CorrectOptionId
            })
            .ToList();
    }

    private static List<QuizAnswerInput> AllWrong(
        List<(int QuestionId, int CorrectOptionId, int WrongOptionId)> key)
    {
        return key
            .Select(x => new QuizAnswerInput
            {
                QuestionId = x.QuestionId,
                OptionId = x.WrongOptionId
            })
            .ToList();
    }

    [Fact]
    public async Task AllCorrectAnswers_Scores100AndPasses()
    {
        var setup = await SetupQuizAsync();
        using var host = setup.Host;

        var quiz = await host.GetQuizByLessonAsync(setup.FirstLessonId);
        var key = await GetAnswerKeyAsync(host, quiz.Id);

        var result = await host.SendAsync(new SubmitQuizAttemptCommand
        {
            LessonId = setup.FirstLessonId,
            Answers = AllCorrect(key)
        });

        Assert.True(result.Success, result.Message);
        Assert.Equal(100m, result.Data!.Score);
        Assert.True(result.Data.IsPassed);
        Assert.Equal(1, result.Data.AttemptNumber);
        Assert.Equal(4, result.Data.Answers.Count);
        Assert.All(result.Data.Answers, a => Assert.Equal(1m, a.PointsAwarded));
    }

    [Fact]
    public async Task HalfCorrectAnswers_Scores50AndFailsAt80Threshold()
    {
        var setup = await SetupQuizAsync();
        using var host = setup.Host;

        var quiz = await host.GetQuizByLessonAsync(setup.FirstLessonId);
        var key = await GetAnswerKeyAsync(host, quiz.Id);

        var answers = key
            .Select((x, index) => new QuizAnswerInput
            {
                QuestionId = x.QuestionId,
                OptionId = index % 2 == 0 ? x.CorrectOptionId : x.WrongOptionId
            })
            .ToList();

        var result = await host.SendAsync(new SubmitQuizAttemptCommand
        {
            LessonId = setup.FirstLessonId,
            Answers = answers
        });

        Assert.True(result.Success, result.Message);
        Assert.Equal(50m, result.Data!.Score);
        Assert.False(result.Data.IsPassed);
    }

    [Fact]
    public async Task CustomThreshold_IsRespectedWhenScoring()
    {
        var setup = await SetupQuizAsync(passingScorePercent: 50m);
        using var host = setup.Host;

        var quiz = await host.GetQuizByLessonAsync(setup.FirstLessonId);
        var key = await GetAnswerKeyAsync(host, quiz.Id);

        var answers = key
            .Select((x, index) => new QuizAnswerInput
            {
                QuestionId = x.QuestionId,
                OptionId = index % 2 == 0 ? x.CorrectOptionId : x.WrongOptionId
            })
            .ToList();

        var result = await host.SendAsync(new SubmitQuizAttemptCommand
        {
            LessonId = setup.FirstLessonId,
            Answers = answers
        });

        Assert.True(result.Success, result.Message);
        Assert.Equal(50m, result.Data!.Score);
        Assert.True(result.Data.IsPassed);
    }

    [Fact]
    public async Task RepeatedSubmissions_IncrementAttemptNumbers()
    {
        var setup = await SetupQuizAsync();
        using var host = setup.Host;

        var quiz = await host.GetQuizByLessonAsync(setup.FirstLessonId);
        var key = await GetAnswerKeyAsync(host, quiz.Id);

        var first = await host.SendAsync(new SubmitQuizAttemptCommand
        {
            LessonId = setup.FirstLessonId,
            Answers = AllWrong(key)
        });

        var second = await host.SendAsync(new SubmitQuizAttemptCommand
        {
            LessonId = setup.FirstLessonId,
            Answers = AllCorrect(key)
        });

        Assert.True(first.Success, first.Message);
        Assert.True(second.Success, second.Message);
        Assert.Equal(1, first.Data!.AttemptNumber);
        Assert.Equal(2, second.Data!.AttemptNumber);
        Assert.False(first.Data.IsPassed);
        Assert.True(second.Data.IsPassed);

        var attempts = await host.SendAsync(
            new GetQuizAttemptsQuery(setup.FirstLessonId));

        Assert.True(attempts.Success, attempts.Message);
        var numbers = attempts.Data!
            .Select(a => a.AttemptNumber)
            .OrderBy(n => n)
            .ToList();
        Assert.Equal(new[] { 1, 2 }, numbers);
    }

    [Fact]
    public async Task IncompleteAnswers_AreRejected()
    {
        var setup = await SetupQuizAsync();
        using var host = setup.Host;

        var quiz = await host.GetQuizByLessonAsync(setup.FirstLessonId);
        var key = await GetAnswerKeyAsync(host, quiz.Id);

        var result = await host.SendAsync(new SubmitQuizAttemptCommand
        {
            LessonId = setup.FirstLessonId,
            Answers = AllCorrect(key).Skip(1).ToList()
        });

        Assert.False(result.Success);
        Assert.Equal(
            QuizErrors.IncompleteAnswers.Code,
            result.Error?.Code);
    }

    [Fact]
    public async Task DuplicateQuestionAnswers_AreRejected()
    {
        var setup = await SetupQuizAsync();
        using var host = setup.Host;

        var quiz = await host.GetQuizByLessonAsync(setup.FirstLessonId);
        var key = await GetAnswerKeyAsync(host, quiz.Id);

        var answers = AllCorrect(key);
        answers.Add(answers[0]);

        var result = await host.SendAsync(new SubmitQuizAttemptCommand
        {
            LessonId = setup.FirstLessonId,
            Answers = answers
        });

        Assert.False(result.Success);
        Assert.Equal(QuizErrors.DuplicateAnswers.Code, result.Error?.Code);
    }

    [Fact]
    public async Task OptionBelongingToDifferentQuestion_IsRejected()
    {
        var setup = await SetupQuizAsync();
        using var host = setup.Host;

        var quiz = await host.GetQuizByLessonAsync(setup.FirstLessonId);
        var key = await GetAnswerKeyAsync(host, quiz.Id);

        var answers = AllCorrect(key);
        answers[0].OptionId = key[1].CorrectOptionId;

        var result = await host.SendAsync(new SubmitQuizAttemptCommand
        {
            LessonId = setup.FirstLessonId,
            Answers = answers
        });

        Assert.False(result.Success);
        Assert.Equal(QuizErrors.InvalidAnswers.Code, result.Error?.Code);
    }

    [Fact]
    public async Task GetQuiz_DoesNotLeakCorrectAnswers()
    {
        var setup = await SetupQuizAsync();
        using var host = setup.Host;

        var result = await host.SendAsync(
            new GetQuizQuery(setup.FirstLessonId));

        Assert.True(result.Success, result.Message);
        Assert.All(
            result.Data!.Questions,
            q => Assert.Equal(2, q.Options.Count));

        Assert.Null(
            typeof(AnswerOptionDto).GetProperty("IsCorrect"));

        var json = JsonSerializer.Serialize(result.Data);
        Assert.DoesNotContain("IsCorrect", json);
    }

    [Fact]
    public async Task GetQuiz_ForLockedLesson_IsRejected()
    {
        var setup = await SetupQuizAsync(unlockSecondLesson: false);
        using var host = setup.Host;

        await host.CreateQuizAsync(setup.SecondLessonId);

        var result = await host.SendAsync(
            new GetQuizQuery(setup.SecondLessonId));

        Assert.False(result.Success);
        Assert.Equal(LessonErrors.LessonLocked.Code, result.Error?.Code);
    }

    [Fact]
    public async Task SubmitQuiz_ForLockedLesson_IsRejected()
    {
        var setup = await SetupQuizAsync(unlockSecondLesson: false);
        using var host = setup.Host;

        var secondQuiz = await host.CreateQuizAsync(setup.SecondLessonId);
        var key = await GetAnswerKeyAsync(host, secondQuiz.Id);

        var result = await host.SendAsync(new SubmitQuizAttemptCommand
        {
            LessonId = setup.SecondLessonId,
            Answers = AllCorrect(key)
        });

        Assert.False(result.Success);
        Assert.Equal(LessonErrors.LessonLocked.Code, result.Error?.Code);
    }

    [Fact]
    public async Task GetQuiz_ForUnenrolledStudent_IsRejected()
    {
        var setup = await SetupQuizAsync();
        using var host = setup.Host;

        var outsider = await host.CreateUserAsync(lastName: "Outsider");
        host.CurrentUser.SignIn(outsider);

        var result = await host.SendAsync(
            new GetQuizQuery(setup.FirstLessonId));

        Assert.False(result.Success);
        Assert.Equal(LessonErrors.NotEnrolled.Code, result.Error?.Code);
    }

    [Fact]
    public async Task AttemptList_ReturnsOnlyCurrentStudentsAttempts()
    {
        var setup = await SetupQuizAsync();
        using var host = setup.Host;

        var quiz = await host.GetQuizByLessonAsync(setup.FirstLessonId);
        var key = await GetAnswerKeyAsync(host, quiz.Id);

        await host.SendAsync(new SubmitQuizAttemptCommand
        {
            LessonId = setup.FirstLessonId,
            Answers = AllCorrect(key)
        });

        var classmate = await host.CreateUserAsync(lastName: "Classmate");
        await host.EnrollAsync(classmate.Id, setup.CourseId);
        host.CurrentUser.SignIn(classmate);

        var classmateAttempts = await host.SendAsync(
            new GetQuizAttemptsQuery(setup.FirstLessonId));

        Assert.True(classmateAttempts.Success, classmateAttempts.Message);
        Assert.Empty(classmateAttempts.Data!);

        host.CurrentUser.SignIn(setup.Student);

        var ownAttempts = await host.SendAsync(
            new GetQuizAttemptsQuery(setup.FirstLessonId));

        Assert.True(ownAttempts.Success, ownAttempts.Message);
        Assert.Single(ownAttempts.Data!);
    }

    [Fact]
    public async Task GetQuizAttempts_ForLessonWithoutQuiz_IsRejected()
    {
        var setup = await SetupQuizAsync();
        using var host = setup.Host;

        var result = await host.SendAsync(
            new GetQuizAttemptsQuery(setup.SecondLessonId));

        Assert.False(result.Success);
        Assert.Equal(QuizErrors.QuizNotFound.Code, result.Error?.Code);
    }
}
