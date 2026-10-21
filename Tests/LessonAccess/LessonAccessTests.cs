using Application.Features.Lessons.DTOs;
using Application.Features.Lessons.Queries.GetCourseLessons;
using Application.Features.Lessons.Queries.GetLessonAccess;
using Application.Services;
using Domain.Entities;
using Tests.TestInfrastructure;
using Xunit;

namespace Tests.LessonAccess;

public class LessonAccessTests : IClassFixture<TestDatabaseFixture>
{
    private readonly TestDatabaseFixture _fixture;

    public LessonAccessTests(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    private static GetLessonAccessQuery Query(int lessonId) =>
        new(lessonId);

    private static async Task<LessonAccessDto> GetAccessAsync(
        TestHost host,
        int lessonId)
    {
        var result = await host.SendAsync(Query(lessonId));

        Assert.True(result.Success, result.Message);
        Assert.NotNull(result.Data);

        return result.Data!;
    }

    private static async Task SeedProgressAsync(
        TestHost host,
        Guid studentId,
        int lessonId,
        decimal watchedPercentage,
        double durationSeconds = 100)
    {
        await host.ExecuteAsync(async context =>
        {
            context.LessonProgresses.Add(new Domain.Entities.LessonProgress
            {
                ApplicationUserId = studentId,
                LessonId = lessonId,
                LastPositionSeconds = durationSeconds,
                DurationSeconds = durationSeconds,
                WatchedPercentage = watchedPercentage,
                IsWatched = watchedPercentage >= 90m,
                WatchedAt = watchedPercentage >= 90m
                    ? DateTime.UtcNow
                    : null,
                UpdatedAt = DateTime.UtcNow
            });

            await context.SaveChangesAsync();
        });
    }

    private static async Task SeedPassedAttemptAsync(
        TestHost host,
        Guid studentId,
        int quizId,
        decimal score,
        bool isPassed)
    {
        await host.ExecuteAsync(async context =>
        {
            context.QuizAttempts.Add(new QuizAttempt
            {
                ApplicationUserId = studentId,
                QuizId = quizId,
                AttemptNumber = 1,
                StartedAt = DateTime.UtcNow,
                SubmittedAt = DateTime.UtcNow,
                Score = score,
                IsPassed = isPassed
            });

            await context.SaveChangesAsync();
        });
    }

    [Fact]
    public async Task FirstLesson_IsAccessibleForEnrolledStudent()
    {
        using var host = new TestHost(_fixture);

        var student = await host.CreateUserAsync();
        var course = await host.CreateCourseAsync();
        var lesson = await host.CreateLessonAsync(course.Id, 0);
        await host.EnrollAsync(student.Id, course.Id);

        host.CurrentUser.SignIn(student);

        var access = await GetAccessAsync(host, lesson.Id);

        Assert.True(access.IsAccessible);
        Assert.True(access.IsEnrolled);
        Assert.Null(access.MissingRequirement);
    }

    [Fact]
    public async Task UnenrolledStudent_CannotAccessAnyLesson()
    {
        using var host = new TestHost(_fixture);

        var student = await host.CreateUserAsync();
        var course = await host.CreateCourseAsync();
        var lesson = await host.CreateLessonAsync(course.Id, 0);

        host.CurrentUser.SignIn(student);

        var access = await GetAccessAsync(host, lesson.Id);

        Assert.False(access.IsAccessible);
        Assert.False(access.IsEnrolled);
        Assert.Equal(
            LessonAccessRequirements.Enrollment,
            access.MissingRequirement);
    }

    [Fact]
    public async Task SecondLesson_IsLockedWithoutPreviousLessonCompletion()
    {
        using var host = new TestHost(_fixture);

        var student = await host.CreateUserAsync();
        var course = await host.CreateCourseAsync();
        await host.CreateLessonAsync(course.Id, 0);
        var lesson2 = await host.CreateLessonAsync(course.Id, 1);
        await host.EnrollAsync(student.Id, course.Id);

        host.CurrentUser.SignIn(student);

        var access = await GetAccessAsync(host, lesson2.Id);

        Assert.False(access.IsAccessible);
        Assert.False(access.IsVideoCompleted);
        Assert.Equal(
            LessonAccessRequirements.Video,
            access.MissingRequirement);
    }

    [Fact]
    public async Task SecondLesson_StaysLockedWhenVideoCoverageBelow90()
    {
        using var host = new TestHost(_fixture);

        var student = await host.CreateUserAsync();
        var course = await host.CreateCourseAsync();
        var lesson1 = await host.CreateLessonAsync(course.Id, 0);
        var lesson2 = await host.CreateLessonAsync(course.Id, 1);
        await host.EnrollAsync(student.Id, course.Id);
        await host.CreateQuizAsync(lesson1.Id);

        await SeedProgressAsync(
            host, student.Id, lesson1.Id, watchedPercentage: 89.99m);

        host.CurrentUser.SignIn(student);

        var access = await GetAccessAsync(host, lesson2.Id);

        Assert.False(access.IsAccessible);
        Assert.False(access.IsVideoCompleted);
        Assert.Equal(
            LessonAccessRequirements.Video,
            access.MissingRequirement);
    }

    [Fact]
    public async Task SecondLesson_StaysLockedWhenQuizScoreBelow80()
    {
        using var host = new TestHost(_fixture);

        var student = await host.CreateUserAsync();
        var course = await host.CreateCourseAsync();
        var lesson1 = await host.CreateLessonAsync(course.Id, 0);
        var lesson2 = await host.CreateLessonAsync(course.Id, 1);
        await host.EnrollAsync(student.Id, course.Id);

        var quiz = await host.CreateQuizAsync(lesson1.Id);

        await SeedProgressAsync(
            host, student.Id, lesson1.Id, watchedPercentage: 95m);

        await SeedPassedAttemptAsync(
            host, student.Id, quiz.Id, score: 75m, isPassed: false);

        host.CurrentUser.SignIn(student);

        var access = await GetAccessAsync(host, lesson2.Id);

        Assert.False(access.IsAccessible);
        Assert.True(access.IsVideoCompleted);
        Assert.False(access.IsQuizPassed);
        Assert.Equal(
            LessonAccessRequirements.Quiz,
            access.MissingRequirement);
    }

    [Fact]
    public async Task SecondLesson_UnlocksWhenVideoAndQuizBothCompleted()
    {
        using var host = new TestHost(_fixture);

        var student = await host.CreateUserAsync();
        var course = await host.CreateCourseAsync();
        var lesson1 = await host.CreateLessonAsync(course.Id, 0);
        var lesson2 = await host.CreateLessonAsync(course.Id, 1);
        await host.EnrollAsync(student.Id, course.Id);

        var quiz = await host.CreateQuizAsync(lesson1.Id);

        await SeedProgressAsync(
            host, student.Id, lesson1.Id, watchedPercentage: 92m);

        await SeedPassedAttemptAsync(
            host, student.Id, quiz.Id, score: 100m, isPassed: true);

        host.CurrentUser.SignIn(student);

        var access = await GetAccessAsync(host, lesson2.Id);

        Assert.True(access.IsAccessible);
        Assert.True(access.IsVideoCompleted);
        Assert.True(access.IsQuizPassed);
        Assert.Null(access.MissingRequirement);
    }

    [Fact]
    public async Task AnotherStudentsCompletion_DoesNotUnlockCurrentStudent()
    {
        using var host = new TestHost(_fixture);

        var studentA = await host.CreateUserAsync(lastName: "Alpha");
        var studentB = await host.CreateUserAsync(lastName: "Beta");
        var course = await host.CreateCourseAsync();
        var lesson1 = await host.CreateLessonAsync(course.Id, 0);
        var lesson2 = await host.CreateLessonAsync(course.Id, 1);

        await host.EnrollAsync(studentA.Id, course.Id);
        await host.EnrollAsync(studentB.Id, course.Id);

        var quiz = await host.CreateQuizAsync(lesson1.Id);

        await SeedProgressAsync(
            host, studentA.Id, lesson1.Id, watchedPercentage: 100m);
        await SeedPassedAttemptAsync(
            host, studentA.Id, quiz.Id, score: 100m, isPassed: true);

        host.CurrentUser.SignIn(studentB);

        var accessForB = await GetAccessAsync(host, lesson2.Id);

        Assert.False(accessForB.IsAccessible);
        Assert.False(accessForB.IsVideoCompleted);
        Assert.Equal(
            LessonAccessRequirements.Video,
            accessForB.MissingRequirement);

        host.CurrentUser.SignIn(studentA);

        var accessForA = await GetAccessAsync(host, lesson2.Id);

        Assert.True(accessForA.IsAccessible);
    }

    [Fact]
    public async Task TeacherRole_BypassesLessonLock()
    {
        using var host = new TestHost(_fixture);

        var teacher = await host.CreateUserAsync(
            firstName: "Tina",
            lastName: "Teacher");
        var course = await host.CreateCourseAsync();
        await host.CreateLessonAsync(course.Id, 0);
        var lesson2 = await host.CreateLessonAsync(course.Id, 1);

        host.CurrentUser.SignIn(
            teacher,
            AppRoles.Teacher);

        var access = await GetAccessAsync(host, lesson2.Id);

        Assert.True(access.IsAccessible);
    }

    [Fact]
    public async Task LessonWithoutVideo_TreatsVideoConditionAsSatisfied()
    {
        using var host = new TestHost(_fixture);

        var student = await host.CreateUserAsync();
        var course = await host.CreateCourseAsync();
        var lesson1 = await host.CreateLessonAsync(
            course.Id, 0, videoId: null);
        var lesson2 = await host.CreateLessonAsync(course.Id, 1);
        await host.EnrollAsync(student.Id, course.Id);

        host.CurrentUser.SignIn(student);

        var access = await GetAccessAsync(host, lesson2.Id);

        Assert.True(access.IsAccessible);
        Assert.True(access.IsVideoCompleted);
        Assert.True(lesson1.YouTubeVideoId is null);
    }

    [Fact]
    public async Task CourseLessonList_ShowsPerStudentUnlockStatus()
    {
        using var host = new TestHost(_fixture);

        var student = await host.CreateUserAsync();
        var course = await host.CreateCourseAsync();
        var lesson1 = await host.CreateLessonAsync(course.Id, 0);
        var lesson2 = await host.CreateLessonAsync(course.Id, 1);
        var lesson3 = await host.CreateLessonAsync(course.Id, 2);
        await host.EnrollAsync(student.Id, course.Id);

        var quiz1 = await host.CreateQuizAsync(lesson1.Id);

        host.CurrentUser.SignIn(student);

        var beforeResult = await host.SendAsync(
            new GetCourseLessonsQuery(course.Id));

        Assert.True(beforeResult.Success);
        Assert.True(beforeResult.Data![0].IsUnlocked);
        Assert.False(beforeResult.Data[1].IsUnlocked);
        Assert.False(beforeResult.Data[2].IsUnlocked);

        await SeedProgressAsync(
            host, student.Id, lesson1.Id, watchedPercentage: 100m);
        await SeedPassedAttemptAsync(
            host, student.Id, quiz1.Id, score: 90m, isPassed: true);

        var afterResult = await host.SendAsync(
            new GetCourseLessonsQuery(course.Id));

        Assert.True(afterResult.Success);
        Assert.True(afterResult.Data![0].IsUnlocked);
        Assert.True(afterResult.Data[0].IsVideoCompleted);
        Assert.True(afterResult.Data[1].IsUnlocked);
        Assert.False(afterResult.Data[2].IsUnlocked);
    }

    [Fact]
    public async Task CourseLessonList_RejectsUnenrolledStudent()
    {
        using var host = new TestHost(_fixture);

        var student = await host.CreateUserAsync();
        var course = await host.CreateCourseAsync();
        await host.CreateLessonAsync(course.Id, 0);

        host.CurrentUser.SignIn(student);

        var result = await host.SendAsync(
            new GetCourseLessonsQuery(course.Id));

        Assert.False(result.Success);
        Assert.Equal("Lesson.NotEnrolled", result.Error?.Code);
    }
}
