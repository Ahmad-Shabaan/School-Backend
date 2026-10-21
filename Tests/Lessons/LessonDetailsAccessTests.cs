using Application.Features.Lessons.Commands.CreateLesson;
using Application.Features.Lessons.DTOs;
using Application.Features.Lessons.Errors;
using Application.Features.Lessons.Queries.GetLessonDetails;
using Domain.Entities;
using Tests.TestInfrastructure;
using Xunit;

namespace Tests.Lessons;

public class LessonDetailsAccessTests : IClassFixture<TestDatabaseFixture>
{
    private readonly TestDatabaseFixture _fixture;

    public LessonDetailsAccessTests(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    private sealed record Setup(
        TestHost Host,
        ApplicationUser Student,
        int CourseId,
        int OpenLessonId,
        int LockedLessonId);

    private async Task<Setup> SetupCourseWithMeetingAsync()
    {
        var host = new TestHost(_fixture);

        var teacher = await host.CreateUserAsync(
            firstName: "Tina",
            lastName: "Teacher");

        var course = await host.CreateCourseAsync();

        host.CurrentUser.SignIn(teacher, AppRoles.Teacher);

        var created = await host.SendAsync(new CreateLessonCommand
        {
            CourseId = course.Id,
            Title = "Meeting lesson",
            Description = "Contains resources",
            YouTubeUrl = "https://www.youtube.com/watch?v=dQw4w9WgXcQ",
            MeetingUrl = "https://meet.example.com/room-1",
            MeetingDateTime = new DateTime(2026, 10, 20, 12, 0, 0,
                DateTimeKind.Utc),
            Goals = new List<string> { "Attend the meeting" },
            Resources = new List<LessonResourceInput>
            {
                new()
                {
                    Title = "Agenda",
                    FileUrl = "https://example.com/agenda.pdf",
                    FileType = "pdf"
                }
            }
        });

        Assert.True(created.Success, created.Message);

        var lockedLesson = await host.CreateLessonAsync(
            course.Id, 1, "Locked");

        var student = await host.CreateUserAsync();
        await host.EnrollAsync(student.Id, course.Id);

        return new Setup(
            host,
            student,
            course.Id,
            created.Data!.Id,
            lockedLesson.Id);
    }

    [Fact]
    public async Task UnenrolledUser_CannotReadLessonResourcesOrMeeting()
    {
        var setup = await SetupCourseWithMeetingAsync();
        using var host = setup.Host;

        var outsider = await host.CreateUserAsync(lastName: "Outsider");
        host.CurrentUser.SignIn(outsider);

        var result = await host.SendAsync(
            new GetLessonDetailsQuery(setup.OpenLessonId));

        Assert.False(result.Success);
        Assert.Equal(LessonErrors.NotEnrolled.Code, result.Error?.Code);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task EnrolledStudent_CannotReadLockedLessonDetails()
    {
        var setup = await SetupCourseWithMeetingAsync();
        using var host = setup.Host;

        host.CurrentUser.SignIn(setup.Student);

        var result = await host.SendAsync(
            new GetLessonDetailsQuery(setup.LockedLessonId));

        Assert.False(result.Success);
        Assert.Equal(LessonErrors.LessonLocked.Code, result.Error?.Code);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task EnrolledStudent_ReadsMeetingAndResourcesOnOpenLesson()
    {
        var setup = await SetupCourseWithMeetingAsync();
        using var host = setup.Host;

        host.CurrentUser.SignIn(setup.Student);

        var result = await host.SendAsync(
            new GetLessonDetailsQuery(setup.OpenLessonId));

        Assert.True(result.Success, result.Message);
        Assert.Equal(
            "https://meet.example.com/room-1",
            result.Data!.MeetingUrl);
        Assert.NotNull(result.Data.MeetingDateTime);
        Assert.Single(result.Data.Goals);
        var resource = Assert.Single(result.Data.Resources);
        Assert.Equal("https://example.com/agenda.pdf", resource.FileUrl);
        Assert.True(result.Data.Access.IsAccessible);
        Assert.True(result.Data.Access.IsEnrolled);
    }
}
