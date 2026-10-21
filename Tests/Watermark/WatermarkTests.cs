using Application.Features.Lessons.Errors;
using Application.Features.Watermark.Queries.GetLessonWatermark;
using Domain.Entities;
using Tests.TestInfrastructure;
using Xunit;

namespace Tests.Watermark;

public class WatermarkTests : IClassFixture<TestDatabaseFixture>
{
    private readonly TestDatabaseFixture _fixture;

    public WatermarkTests(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Watermark_ReturnsMaskedIdentityForEnrolledStudent()
    {
        using var host = new TestHost(_fixture);

        var student = await host.CreateUserAsync();
        var course = await host.CreateCourseAsync();
        var lesson = await host.CreateLessonAsync(course.Id, 0);
        await host.EnrollAsync(student.Id, course.Id);

        host.CurrentUser.SignIn(student);

        var result = await host.SendAsync(
            new GetLessonWatermarkQuery(lesson.Id));

        Assert.True(result.Success, result.Message);
        Assert.Equal("Test Student", result.Data!.DisplayName);
        Assert.Equal(student.Id.ToString(), result.Data.StudentId);

        var email = student.Email!;
        var atIndex = email.IndexOf('@');
        var localPart = email[..atIndex];
        var domain = email[atIndex..];
        Assert.Equal($"{localPart[0]}***{domain}", result.Data.MaskedEmail);
        Assert.DoesNotContain(localPart, result.Data.MaskedEmail);
    }

    [Fact]
    public async Task Watermark_WhenNotEnrolled_IsRejected()
    {
        using var host = new TestHost(_fixture);

        var student = await host.CreateUserAsync();
        var course = await host.CreateCourseAsync();
        var lesson = await host.CreateLessonAsync(course.Id, 0);

        host.CurrentUser.SignIn(student);

        var result = await host.SendAsync(
            new GetLessonWatermarkQuery(lesson.Id));

        Assert.False(result.Success);
        Assert.Equal(LessonErrors.NotEnrolled.Code, result.Error?.Code);
    }

    [Fact]
    public async Task Watermark_OnLockedLesson_IsRejected()
    {
        using var host = new TestHost(_fixture);

        var student = await host.CreateUserAsync();
        var course = await host.CreateCourseAsync();
        await host.CreateLessonAsync(course.Id, 0);
        var lockedLesson = await host.CreateLessonAsync(course.Id, 1);
        await host.EnrollAsync(student.Id, course.Id);

        host.CurrentUser.SignIn(student);

        var result = await host.SendAsync(
            new GetLessonWatermarkQuery(lockedLesson.Id));

        Assert.False(result.Success);
        Assert.Equal(LessonErrors.LessonLocked.Code, result.Error?.Code);
    }

    [Fact]
    public async Task TeacherBypassesLockOnWatermark()
    {
        using var host = new TestHost(_fixture);

        var teacher = await host.CreateUserAsync(
            firstName: "Tina",
            lastName: "Teacher");

        var course = await host.CreateCourseAsync();
        await host.CreateLessonAsync(course.Id, 0);
        var lockedLesson = await host.CreateLessonAsync(course.Id, 1);

        host.CurrentUser.SignIn(teacher, AppRoles.Teacher);

        var result = await host.SendAsync(
            new GetLessonWatermarkQuery(lockedLesson.Id));

        Assert.True(result.Success, result.Message);
        Assert.Equal("Tina Teacher", result.Data!.DisplayName);
        Assert.Equal(teacher.Id.ToString(), result.Data.StudentId);
    }
}
