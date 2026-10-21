using Application.Features.Comments.Commands.CreateComment;
using Application.Features.Comments.Commands.DeleteComment;
using Application.Features.Comments.Commands.UpdateComment;
using Application.Features.Comments.Errors;
using Application.Features.Comments.Queries.GetLessonComments;
using Application.Features.Lessons.Errors;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Tests.TestInfrastructure;
using Xunit;

namespace Tests.Comments;

public class CommentTests : IClassFixture<TestDatabaseFixture>
{
    private readonly TestDatabaseFixture _fixture;

    public CommentTests(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    private sealed record Setup(
        TestHost Host,
        ApplicationUser Student,
        int FirstLessonId,
        int SecondLessonId);

    private async Task<Setup> SetupUnlockedCourseAsync()
    {
        var host = new TestHost(_fixture);

        var student = await host.CreateUserAsync();
        var course = await host.CreateCourseAsync();
        var lesson1 = await host.CreateLessonAsync(course.Id, 0);
        var lesson2 = await host.CreateLessonAsync(course.Id, 1);

        await host.EnrollAsync(student.Id, course.Id);

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

        host.CurrentUser.SignIn(student);

        return new Setup(host, student, lesson1.Id, lesson2.Id);
    }

    private static CreateCommentCommand Comment(
        int lessonId,
        string content,
        int? parentCommentId = null)
    {
        return new CreateCommentCommand
        {
            LessonId = lessonId,
            Content = content,
            ParentCommentId = parentCommentId
        };
    }

    [Fact]
    public async Task CreateTopLevelComment_ReturnsAuthorName()
    {
        var setup = await SetupUnlockedCourseAsync();
        using var host = setup.Host;

        var result = await host.SendAsync(
            Comment(setup.FirstLessonId, "Great lesson!"));

        Assert.True(result.Success, result.Message);
        Assert.Equal("Great lesson!", result.Data!.Content);
        Assert.Equal("Test Student", result.Data.AuthorName);
        Assert.Equal(setup.Student.Id, result.Data.AuthorId);
        Assert.Null(result.Data.ParentCommentId);
    }

    [Fact]
    public async Task CreateReply_NestsUnderParentComment()
    {
        var setup = await SetupUnlockedCourseAsync();
        using var host = setup.Host;

        var parent = await host.SendAsync(
            Comment(setup.FirstLessonId, "Top level"));

        var reply = await host.SendAsync(
            Comment(setup.FirstLessonId, "A reply", parent.Data!.Id));

        Assert.True(reply.Success, reply.Message);
        Assert.Equal(parent.Data!.Id, reply.Data!.ParentCommentId);

        var list = await host.SendAsync(
            new GetLessonCommentsQuery(setup.FirstLessonId));

        Assert.True(list.Success, list.Message);
        var single = Assert.Single(list.Data!);
        var nested = Assert.Single(single.Replies);
        Assert.Equal("A reply", nested.Content);
    }

    [Fact]
    public async Task ReplyToReply_IsRejected()
    {
        var setup = await SetupUnlockedCourseAsync();
        using var host = setup.Host;

        var parent = await host.SendAsync(
            Comment(setup.FirstLessonId, "Top level"));
        var reply = await host.SendAsync(
            Comment(setup.FirstLessonId, "A reply", parent.Data!.Id));

        var nested = await host.SendAsync(
            Comment(
                setup.FirstLessonId,
                "Too deep",
                reply.Data!.Id));

        Assert.False(nested.Success);
        Assert.Equal(CommentErrors.ReplyNotAllowed.Code, nested.Error?.Code);
    }

    [Fact]
    public async Task ParentCommentFromDifferentLesson_IsRejected()
    {
        var setup = await SetupUnlockedCourseAsync();
        using var host = setup.Host;

        var foreignParent = await host.SendAsync(
            Comment(setup.SecondLessonId, "On lesson 2"));

        var result = await host.SendAsync(
            Comment(
                setup.FirstLessonId,
                "Cross lesson reply",
                foreignParent.Data!.Id));

        Assert.False(result.Success);
        Assert.Equal(CommentErrors.InvalidParent.Code, result.Error?.Code);
    }

    [Fact]
    public async Task UpdateComment_ByNonAuthor_IsRejected()
    {
        var setup = await SetupUnlockedCourseAsync();
        using var host = setup.Host;

        var comment = await host.SendAsync(
            Comment(setup.FirstLessonId, "Original"));

        var other = await host.CreateUserAsync(lastName: "Other");
        host.CurrentUser.SignIn(other);

        var result = await host.SendAsync(new UpdateCommentCommand
        {
            Id = comment.Data!.Id,
            Content = "Hijacked"
        });

        Assert.False(result.Success);
        Assert.Equal(CommentErrors.NotCommentAuthor.Code, result.Error?.Code);
    }

    [Fact]
    public async Task UpdateComment_ByAuthor_UpdatesContentAndTimestamp()
    {
        var setup = await SetupUnlockedCourseAsync();
        using var host = setup.Host;

        var comment = await host.SendAsync(
            Comment(setup.FirstLessonId, "Original"));

        var result = await host.SendAsync(new UpdateCommentCommand
        {
            Id = comment.Data!.Id,
            Content = "Edited text"
        });

        Assert.True(result.Success, result.Message);
        Assert.Equal("Edited text", result.Data!.Content);
        Assert.NotNull(result.Data.UpdatedAt);

        var list = await host.SendAsync(
            new GetLessonCommentsQuery(setup.FirstLessonId));

        Assert.Equal("Edited text", Assert.Single(list.Data!).Content);
    }

    [Fact]
    public async Task UpdateComment_NotFound_ReturnsNotFound()
    {
        var setup = await SetupUnlockedCourseAsync();
        using var host = setup.Host;

        var result = await host.SendAsync(new UpdateCommentCommand
        {
            Id = 999999,
            Content = "Ghost edit"
        });

        Assert.False(result.Success);
        Assert.Equal(CommentErrors.CommentNotFound.Code, result.Error?.Code);
    }

    [Fact]
    public async Task DeleteComment_ByUnrelatedStudent_IsRejected()
    {
        var setup = await SetupUnlockedCourseAsync();
        using var host = setup.Host;

        var comment = await host.SendAsync(
            Comment(setup.FirstLessonId, "Mine"));

        var other = await host.CreateUserAsync(lastName: "Other");
        host.CurrentUser.SignIn(other);

        var result = await host.SendAsync(
            new DeleteCommentCommand(comment.Data!.Id));

        Assert.False(result.Success);
        Assert.Equal(
            CommentErrors.NotCommentAuthor.Code,
            result.Error?.Code);

        var stillThere = await host.ExecuteAsync(context =>
            context.LessonComments
                .Where(c => c.Id == comment.Data.Id)
                .SingleAsync());

        Assert.False(stillThere.IsDeleted);
    }

    [Fact]
    public async Task DeleteComment_ByAuthor_SoftDeletesAndHidesReplies()
    {
        var setup = await SetupUnlockedCourseAsync();
        using var host = setup.Host;

        var parent = await host.SendAsync(
            Comment(setup.FirstLessonId, "Parent"));
        var reply = await host.SendAsync(
            Comment(setup.FirstLessonId, "Reply", parent.Data!.Id));

        var result = await host.SendAsync(
            new DeleteCommentCommand(parent.Data!.Id));

        Assert.True(result.Success, result.Message);

        var states = await host.ExecuteAsync(context =>
            context.LessonComments
                .Where(c =>
                    c.Id == parent.Data.Id ||
                    c.Id == reply.Data!.Id)
                .ToListAsync());

        Assert.All(states, c => Assert.True(c.IsDeleted));

        var list = await host.SendAsync(
            new GetLessonCommentsQuery(setup.FirstLessonId));

        Assert.True(list.Success, list.Message);
        Assert.Empty(list.Data!);
        Assert.Equal(0, list.Pagination!.TotalCount);
    }

    [Fact]
    public async Task DeleteComment_ByTeacher_Succeeds()
    {
        var setup = await SetupUnlockedCourseAsync();
        using var host = setup.Host;

        var comment = await host.SendAsync(
            Comment(setup.FirstLessonId, "Student wrote this"));

        var teacher = await host.CreateUserAsync(
            firstName: "Tina",
            lastName: "Teacher");

        host.CurrentUser.SignIn(teacher, AppRoles.Teacher);

        var result = await host.SendAsync(
            new DeleteCommentCommand(comment.Data!.Id));

        Assert.True(result.Success, result.Message);

        var list = await host.SendAsync(
            new GetLessonCommentsQuery(setup.FirstLessonId));

        Assert.Empty(list.Data!);
    }

    [Fact]
    public async Task CreateComment_OnLockedLesson_IsRejected()
    {
        using var host = new TestHost(_fixture);

        var student = await host.CreateUserAsync();
        var course = await host.CreateCourseAsync();
        await host.CreateLessonAsync(course.Id, 0);
        var lockedLesson = await host.CreateLessonAsync(course.Id, 1);
        await host.EnrollAsync(student.Id, course.Id);

        host.CurrentUser.SignIn(student);

        var result = await host.SendAsync(
            Comment(lockedLesson.Id, "Too early"));

        Assert.False(result.Success);
        Assert.Equal(LessonErrors.LessonLocked.Code, result.Error?.Code);
    }

    [Fact]
    public async Task CreateComment_WhenNotEnrolled_IsRejected()
    {
        using var host = new TestHost(_fixture);

        var student = await host.CreateUserAsync();
        var course = await host.CreateCourseAsync();
        var lesson = await host.CreateLessonAsync(course.Id, 0);

        host.CurrentUser.SignIn(student);

        var result = await host.SendAsync(
            Comment(lesson.Id, "No access"));

        Assert.False(result.Success);
        Assert.Equal(LessonErrors.NotEnrolled.Code, result.Error?.Code);
    }

    [Fact]
    public async Task Pagination_CountsOnlyTopLevelComments()
    {
        var setup = await SetupUnlockedCourseAsync();
        using var host = setup.Host;

        var first = await host.SendAsync(
            Comment(setup.FirstLessonId, "First"));
        await host.SendAsync(Comment(setup.FirstLessonId, "Second"));
        await host.SendAsync(Comment(setup.FirstLessonId, "Third"));
        await host.SendAsync(
            Comment(setup.FirstLessonId, "Nested", first.Data!.Id));

        var page = await host.SendAsync(
            new GetLessonCommentsQuery(
                setup.FirstLessonId,
                pageIndex: 0,
                pageSize: 2));

        Assert.True(page.Success, page.Message);
        Assert.Equal(2, page.Data!.Count);
        Assert.Equal(3, page.Pagination!.TotalCount);
        Assert.Equal(2, page.Pagination.TotalPages);

        var secondPage = await host.SendAsync(
            new GetLessonCommentsQuery(
                setup.FirstLessonId,
                pageIndex: 1,
                pageSize: 2));

        Assert.True(secondPage.Success, secondPage.Message);
        Assert.Single(secondPage.Data!);
        Assert.Equal(3, secondPage.Pagination!.TotalCount);
    }

    [Fact]
    public async Task EmptyOrWhitespaceContent_ThrowsValidationException()
    {
        var setup = await SetupUnlockedCourseAsync();
        using var host = setup.Host;

        await Assert.ThrowsAsync<FluentValidation.ValidationException>(
            () => host.SendAsync(
                Comment(setup.FirstLessonId, string.Empty)));

        await Assert.ThrowsAsync<FluentValidation.ValidationException>(
            () => host.SendAsync(
                Comment(setup.FirstLessonId, "   ")));
    }
}
