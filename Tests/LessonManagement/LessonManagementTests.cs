using Application.Features.Lessons.Commands.CreateLesson;
using Application.Features.Lessons.Commands.DeleteLesson;
using Application.Features.Lessons.Commands.ReorderLessons;
using Application.Features.Lessons.DTOs;
using Application.Features.Lessons.Errors;
using Application.Features.Lessons.Queries.GetCourseLessons;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Tests.TestInfrastructure;
using Xunit;

namespace Tests.LessonManagement;

public class LessonManagementTests : IClassFixture<TestDatabaseFixture>
{
    private readonly TestDatabaseFixture _fixture;

    public LessonManagementTests(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    private async Task<(TestHost Host, ApplicationUser Teacher, int CourseId, Lesson A, Lesson B, Lesson C)>
        SetupStaffCourseAsync()
    {
        var host = new TestHost(_fixture);

        var teacher = await host.CreateUserAsync(
            firstName: "Tina",
            lastName: "Teacher");

        var course = await host.CreateCourseAsync();

        var lessonA = await host.CreateLessonAsync(course.Id, 0, "Alpha");
        var lessonB = await host.CreateLessonAsync(course.Id, 1, "Beta");
        var lessonC = await host.CreateLessonAsync(course.Id, 2, "Gamma");

        host.CurrentUser.SignIn(teacher, AppRoles.Teacher);

        return (host, teacher, course.Id, lessonA, lessonB, lessonC);
    }

    private static Task<List<Lesson>> GetLessonsFromDbAsync(
        TestHost host,
        int courseId)
    {
        return host.ExecuteAsync(context =>
            context.Lessons
                .Where(l => l.CourseId == courseId)
                .OrderBy(l => l.Order)
                .ToListAsync());
    }

    [Fact]
    public async Task Student_CannotCreateReorderOrDeleteLessons()
    {
        using var host = new TestHost(_fixture);

        var student = await host.CreateUserAsync();
        var course = await host.CreateCourseAsync();
        var lesson = await host.CreateLessonAsync(course.Id, 0);

        host.CurrentUser.SignIn(student);

        var create = await host.SendAsync(new CreateLessonCommand
        {
            CourseId = course.Id,
            Title = "Nope",
            Description = "Should not be created"
        });

        Assert.False(create.Success);
        Assert.Equal(LessonErrors.NotAuthorized.Code, create.Error?.Code);

        var reorder = await host.SendAsync(new ReorderLessonsCommand
        {
            CourseId = course.Id,
            OrderedLessonIds = new List<int> { lesson.Id }
        });

        Assert.False(reorder.Success);
        Assert.Equal(LessonErrors.NotAuthorized.Code, reorder.Error?.Code);

        var delete = await host.SendAsync(
            new DeleteLessonCommand(lesson.Id));

        Assert.False(delete.Success);
        Assert.Equal(LessonErrors.NotAuthorized.Code, delete.Error?.Code);
    }

    [Fact]
    public async Task TeacherCreatesLesson_AppendsNextOrderAndParsesVideo()
    {
        var (host, _, courseId, _, _, _) = await SetupStaffCourseAsync();
        using var _1 = host;

        var result = await host.SendAsync(new CreateLessonCommand
        {
            CourseId = courseId,
            Title = "Delta",
            Description = "Fourth lesson",
            YouTubeUrl = "https://www.youtube.com/watch?v=dQw4w9WgXcQ&t=5",
            MeetingUrl = "https://meet.example.com/room-42",
            Goals = new List<string> { "Learn things", "Ship them" },
            Resources = new List<LessonResourceInput>
            {
                new()
                {
                    Title = "Slides",
                    FileUrl = "https://example.com/slides.pdf",
                    FileType = "pdf"
                }
            }
        });

        Assert.True(result.Success, result.Message);
        Assert.Equal(3, result.Data!.Order);
        Assert.Equal("dQw4w9WgXcQ", result.Data.YouTubeVideoId);
        Assert.Equal(2, result.Data.Goals.Count);
        Assert.Single(result.Data.Resources);
        Assert.False(result.Data.HasQuiz);

        var lessons = await GetLessonsFromDbAsync(host, courseId);
        Assert.Equal(4, lessons.Count);
        Assert.Equal("Delta", lessons[3].Title);
    }

    [Fact]
    public async Task CreateLesson_WithInvalidYouTubeUrl_IsRejected()
    {
        var (host, _, courseId, _, _, _) = await SetupStaffCourseAsync();
        using var _1 = host;

        var result = await host.SendAsync(new CreateLessonCommand
        {
            CourseId = courseId,
            Title = "Bad video",
            Description = "Invalid source",
            YouTubeUrl = "https://example.com/video/123"
        });

        Assert.False(result.Success);
        Assert.Equal(
            LessonErrors.InvalidYouTubeUrl.Code,
            result.Error?.Code);
    }

    [Fact]
    public async Task CreateLesson_WithInvalidMeetingUrl_IsRejected()
    {
        var (host, _, courseId, _, _, _) = await SetupStaffCourseAsync();
        using var _1 = host;

        var result = await host.SendAsync(new CreateLessonCommand
        {
            CourseId = courseId,
            Title = "Bad meeting",
            Description = "Invalid url",
            MeetingUrl = "not-a-url"
        });

        Assert.False(result.Success);
        Assert.Equal(
            LessonErrors.InvalidMeetingUrl.Code,
            result.Error?.Code);
    }

    [Fact]
    public async Task CreateLesson_ForMissingCourse_IsRejected()
    {
        var (host, _, _, _, _, _) = await SetupStaffCourseAsync();
        using var _1 = host;

        var result = await host.SendAsync(new CreateLessonCommand
        {
            CourseId = 999999,
            Title = "Ghost",
            Description = "No course"
        });

        Assert.False(result.Success);
        Assert.Equal(LessonErrors.CourseNotFound.Code, result.Error?.Code);
    }

    [Fact]
    public async Task Reorder_PersistsNewSequence()
    {
        var (host, _, courseId, a, b, c) = await SetupStaffCourseAsync();
        using var _1 = host;

        var result = await host.SendAsync(new ReorderLessonsCommand
        {
            CourseId = courseId,
            OrderedLessonIds = new List<int> { c.Id, b.Id, a.Id }
        });

        Assert.True(result.Success, result.Message);

        var lessons = await GetLessonsFromDbAsync(host, courseId);
        Assert.Equal(
            new[] { c.Id, b.Id, a.Id },
            lessons.Select(l => l.Id).ToArray());
        Assert.Equal(
            new[] { 0, 1, 2 },
            lessons.Select(l => l.Order).ToArray());

        var list = await host.SendAsync(
            new GetCourseLessonsQuery(courseId));

        Assert.True(list.Success, list.Message);
        Assert.Equal(
            new[] { c.Id, b.Id, a.Id },
            list.Data!.Select(l => l.Id).ToArray());
    }

    [Fact]
    public async Task Reorder_WithMismatchedPayload_IsRejected()
    {
        var (host, _, courseId, a, b, c) = await SetupStaffCourseAsync();
        using var _1 = host;

        var missingOne = await host.SendAsync(new ReorderLessonsCommand
        {
            CourseId = courseId,
            OrderedLessonIds = new List<int> { b.Id, a.Id }
        });

        Assert.False(missingOne.Success);
        Assert.Equal(LessonErrors.InvalidReorder.Code, missingOne.Error?.Code);

        var duplicated = await host.SendAsync(new ReorderLessonsCommand
        {
            CourseId = courseId,
            OrderedLessonIds = new List<int> { c.Id, a.Id, a.Id }
        });

        Assert.False(duplicated.Success);
        Assert.Equal(LessonErrors.InvalidReorder.Code, duplicated.Error?.Code);

        var lessons = await GetLessonsFromDbAsync(host, courseId);
        Assert.Equal(
            new[] { a.Id, b.Id, c.Id },
            lessons.Select(l => l.Id).ToArray());
    }

    [Fact]
    public async Task Reorder_ForMissingCourse_IsRejected()
    {
        var (host, _, _, a, b, _) = await SetupStaffCourseAsync();
        using var _1 = host;

        var result = await host.SendAsync(new ReorderLessonsCommand
        {
            CourseId = 999999,
            OrderedLessonIds = new List<int> { a.Id, b.Id }
        });

        Assert.False(result.Success);
        Assert.Equal(LessonErrors.CourseNotFound.Code, result.Error?.Code);
    }

    [Fact]
    public async Task DeleteMiddleLesson_RenumbersRemainingLessons()
    {
        var (host, _, courseId, a, b, c) = await SetupStaffCourseAsync();
        using var _1 = host;

        var result = await host.SendAsync(
            new DeleteLessonCommand(b.Id));

        Assert.True(result.Success, result.Message);

        var lessons = await GetLessonsFromDbAsync(host, courseId);
        Assert.Equal(
            new[] { a.Id, c.Id },
            lessons.Select(l => l.Id).ToArray());
        Assert.Equal(
            new[] { 0, 1 },
            lessons.Select(l => l.Order).ToArray());

        var list = await host.SendAsync(
            new GetCourseLessonsQuery(courseId));

        Assert.True(list.Success, list.Message);
        Assert.Equal(2, list.Data!.Count);
    }

    [Fact]
    public async Task DeleteMissingLesson_IsRejected()
    {
        var (host, _, _, _, _, _) = await SetupStaffCourseAsync();
        using var _1 = host;

        var result = await host.SendAsync(
            new DeleteLessonCommand(999999));

        Assert.False(result.Success);
        Assert.Equal(LessonErrors.LessonNotFound.Code, result.Error?.Code);
    }
}
