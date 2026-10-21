using Application.Features.Lessons.Errors;
using Application.Features.Progress.Commands.SubmitProgressReport;
using Application.Features.Progress.Errors;
using Application.Features.Progress.Queries.GetLessonProgress;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Tests.TestInfrastructure;
using Xunit;

namespace Tests.Progress;

public class SubmitProgressReportTests : IClassFixture<TestDatabaseFixture>
{
    private readonly TestDatabaseFixture _fixture;

    public SubmitProgressReportTests(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    private async Task<(TestHost Host, ApplicationUser Student, int LessonId)>
        SetupEnrolledLessonAsync(bool withVideo = true)
    {
        var host = new TestHost(_fixture);

        var student = await host.CreateUserAsync();
        var course = await host.CreateCourseAsync();
        var lesson = await host.CreateLessonAsync(
            course.Id,
            0,
            videoId: withVideo ? "dQw4w9WgXcQ" : null);

        await host.EnrollAsync(student.Id, course.Id);

        host.CurrentUser.SignIn(student);

        return (host, student, lesson.Id);
    }

    private static SubmitProgressReportCommand Report(
        int lessonId,
        double position,
        double duration,
        double? segmentStart = null,
        double? segmentEnd = null)
    {
        return new SubmitProgressReportCommand
        {
            LessonId = lessonId,
            CurrentTimeSeconds = position,
            DurationSeconds = duration,
            WatchedSegmentStartSeconds = segmentStart,
            WatchedSegmentEndSeconds = segmentEnd
        };
    }

    private static Task<int> CountSegmentsAsync(
        TestHost host,
        Guid studentId,
        int lessonId)
    {
        return host.ExecuteAsync(context =>
            context.LessonWatchSegments.CountAsync(s =>
                s.ApplicationUserId == studentId &&
                s.LessonId == lessonId));
    }

    [Fact]
    public async Task FirstReport_CreatesProgressWithZeroCoverage()
    {
        var (host, student, lessonId) = await SetupEnrolledLessonAsync();
        using var _ = host;

        var result = await host.SendAsync(
            Report(lessonId, position: 5, duration: 100));

        Assert.True(result.Success, result.Message);
        Assert.Equal(0m, result.Data!.WatchedPercentage);
        Assert.False(result.Data.IsVideoCompleted);
        Assert.Equal(5d, result.Data.LastPositionSeconds);
        Assert.Equal(0, await CountSegmentsAsync(host, student.Id, lessonId));
    }

    [Fact]
    public async Task OverlappingSegments_AreMergedWithoutDoubleCounting()
    {
        var (host, student, lessonId) = await SetupEnrolledLessonAsync();
        using var _ = host;

        var first = await host.SendAsync(
            Report(lessonId, position: 30, duration: 100,
                segmentStart: 0, segmentEnd: 30));

        Assert.True(first.Success, first.Message);
        Assert.Equal(30m, first.Data!.WatchedPercentage);

        var second = await host.SendAsync(
            Report(lessonId, position: 60, duration: 100,
                segmentStart: 20, segmentEnd: 60));

        Assert.True(second.Success, second.Message);
        Assert.Equal(60m, second.Data!.WatchedPercentage);
        Assert.Equal(2, await CountSegmentsAsync(host, student.Id, lessonId));
    }

    [Fact]
    public async Task DuplicateSegment_IsNotReInserted()
    {
        var (host, student, lessonId) = await SetupEnrolledLessonAsync();
        using var _ = host;

        await host.SendAsync(
            Report(lessonId, position: 40, duration: 100,
                segmentStart: 0, segmentEnd: 40));

        var duplicate = await host.SendAsync(
            Report(lessonId, position: 40, duration: 100,
                segmentStart: 0, segmentEnd: 40));

        Assert.True(duplicate.Success, duplicate.Message);
        Assert.Equal(40m, duplicate.Data!.WatchedPercentage);
        Assert.Equal(1, await CountSegmentsAsync(host, student.Id, lessonId));
    }

    [Fact]
    public async Task ReachingThreshold_MarksVideoCompleted()
    {
        var (host, student, lessonId) = await SetupEnrolledLessonAsync();
        using var _ = host;

        var result = await host.SendAsync(
            Report(lessonId, position: 90, duration: 100,
                segmentStart: 0, segmentEnd: 90));

        Assert.True(result.Success, result.Message);
        Assert.Equal(90m, result.Data!.WatchedPercentage);
        Assert.True(result.Data.IsVideoCompleted);

        var progress = await host.SendAsync(
            new GetLessonProgressQuery(lessonId));

        Assert.True(progress.Success, progress.Message);
        Assert.True(progress.Data!.IsVideoCompleted);
        Assert.Equal(90m, progress.Data.WatchedPercentage);
    }

    [Fact]
    public async Task CoverageJustBelowThreshold_DoesNotCompleteVideo()
    {
        var (host, _, lessonId) = await SetupEnrolledLessonAsync();
        using var _ = host;

        var result = await host.SendAsync(
            Report(lessonId, position: 89.9, duration: 100,
                segmentStart: 0, segmentEnd: 89.9));

        Assert.True(result.Success, result.Message);
        Assert.True(result.Data!.WatchedPercentage < 90m);
        Assert.False(result.Data.IsVideoCompleted);
    }

    [Fact]
    public async Task SeekWithoutSegment_DoesNotAddCoverage()
    {
        var (host, student, lessonId) = await SetupEnrolledLessonAsync();
        using var _ = host;

        await host.SendAsync(
            Report(lessonId, position: 30, duration: 100,
                segmentStart: 0, segmentEnd: 30));

        var seek = await host.SendAsync(
            Report(lessonId, position: 100, duration: 100));

        Assert.True(seek.Success, seek.Message);
        Assert.Equal(30m, seek.Data!.WatchedPercentage);
        Assert.Equal(100d, seek.Data.LastPositionSeconds);
        Assert.False(seek.Data.IsVideoCompleted);
        Assert.Equal(1, await CountSegmentsAsync(host, student.Id, lessonId));
    }

    [Fact]
    public async Task Progress_PersistsAcrossNewHostScopes()
    {
        var (host, _, lessonId) = await SetupEnrolledLessonAsync();

        var submitted = await host.SendAsync(
            Report(lessonId, position: 60, duration: 100,
                segmentStart: 0, segmentEnd: 60));

        Assert.True(submitted.Success, submitted.Message);

        using var secondHost = new TestHost(_fixture);
        secondHost.CurrentUser.SignIn(new ApplicationUser
        {
            Id = host.CurrentUser.UserId,
            FirstName = "Test",
            LastName = "Student",
            Email = host.CurrentUser.Email
        });

        var persisted = await secondHost.SendAsync(
            new GetLessonProgressQuery(lessonId));

        Assert.True(persisted.Success, persisted.Message);
        Assert.Equal(60m, persisted.Data!.WatchedPercentage);
        Assert.Equal(60d, persisted.Data.LastPositionSeconds);
        Assert.Equal(100d, persisted.Data.DurationSeconds);
        host.Dispose();
    }

    [Fact]
    public async Task DurationAboveMaximum_IsRejected()
    {
        var (host, _, lessonId) = await SetupEnrolledLessonAsync();
        using var _ = host;

        var result = await host.SendAsync(
            Report(lessonId, position: 10, duration: 50000));

        Assert.False(result.Success);
        Assert.Equal(ProgressErrors.InvalidDuration.Code, result.Error?.Code);
    }

    [Fact]
    public async Task NonPositiveDuration_ThrowsValidationException()
    {
        var (host, _, lessonId) = await SetupEnrolledLessonAsync();
        using var _ = host;

        await Assert.ThrowsAsync<FluentValidation.ValidationException>(
            () => host.SendAsync(
                Report(lessonId, position: 10, duration: 0)));
    }

    [Fact]
    public async Task PositionBeyondDurationTolerance_IsRejected()
    {
        var (host, _, lessonId) = await SetupEnrolledLessonAsync();
        using var _ = host;

        var result = await host.SendAsync(
            Report(lessonId, position: 110, duration: 100));

        Assert.False(result.Success);
        Assert.Equal(ProgressErrors.InvalidPosition.Code, result.Error?.Code);
    }

    [Fact]
    public async Task PositionWithinTolerance_IsAccepted()
    {
        var (host, _, lessonId) = await SetupEnrolledLessonAsync();
        using var _ = host;

        var result = await host.SendAsync(
            Report(lessonId, position: 103, duration: 100));

        Assert.True(result.Success, result.Message);
        Assert.Equal(103d, result.Data!.LastPositionSeconds);
    }

    [Fact]
    public async Task SegmentEndingBeyondPositionTolerance_IsRejected()
    {
        var (host, _, lessonId) = await SetupEnrolledLessonAsync();
        using var _ = host;

        var result = await host.SendAsync(
            Report(lessonId, position: 50, duration: 100,
                segmentStart: 40, segmentEnd: 60));

        Assert.False(result.Success);
        Assert.Equal(ProgressErrors.InvalidSegment.Code, result.Error?.Code);
    }

    [Fact]
    public async Task SegmentLongerThanPeriodLimit_IsRejected()
    {
        var (host, _, lessonId) = await SetupEnrolledLessonAsync();
        using var _ = host;

        var result = await host.SendAsync(
            Report(lessonId, position: 500, duration: 1000,
                segmentStart: 0, segmentEnd: 121));

        Assert.False(result.Success);
        Assert.Equal(ProgressErrors.SegmentTooLong.Code, result.Error?.Code);
    }

    [Fact]
    public async Task SegmentWithoutBothBounds_ThrowsValidationException()
    {
        var (host, _, lessonId) = await SetupEnrolledLessonAsync();
        using var _ = host;

        await Assert.ThrowsAsync<FluentValidation.ValidationException>(
            () => host.SendAsync(
                Report(lessonId, position: 50, duration: 100,
                    segmentStart: 0, segmentEnd: null)));
    }

    [Fact]
    public async Task LessonWithoutVideo_IsRejected()
    {
        var (host, _, lessonId) = await SetupEnrolledLessonAsync(
            withVideo: false);
        using var _ = host;

        var result = await host.SendAsync(
            Report(lessonId, position: 10, duration: 100));

        Assert.False(result.Success);
        Assert.Equal(
            ProgressErrors.LessonHasNoVideo.Code,
            result.Error?.Code);
    }

    [Fact]
    public async Task NotEnrolledStudent_IsRejected()
    {
        using var host = new TestHost(_fixture);

        var student = await host.CreateUserAsync();
        var course = await host.CreateCourseAsync();
        var lesson = await host.CreateLessonAsync(course.Id, 0);

        host.CurrentUser.SignIn(student);

        var result = await host.SendAsync(
            Report(lesson.Id, position: 10, duration: 100));

        Assert.False(result.Success);
        Assert.Equal(LessonErrors.NotEnrolled.Code, result.Error?.Code);
    }

    [Fact]
    public async Task LockedLesson_IsRejected()
    {
        using var host = new TestHost(_fixture);

        var student = await host.CreateUserAsync();
        var course = await host.CreateCourseAsync();
        await host.CreateLessonAsync(course.Id, 0);
        var lockedLesson = await host.CreateLessonAsync(course.Id, 1);
        await host.EnrollAsync(student.Id, course.Id);

        host.CurrentUser.SignIn(student);

        var result = await host.SendAsync(
            Report(lockedLesson.Id, position: 10, duration: 100));

        Assert.False(result.Success);
        Assert.Equal(LessonErrors.LessonLocked.Code, result.Error?.Code);
    }
}
