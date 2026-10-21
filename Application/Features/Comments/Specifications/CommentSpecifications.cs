using Application.Specifications;
using Domain.Entities;

namespace Application.Features.Comments.Specifications;

public class CommentsByLessonSpecification : BaseSpecification<LessonComment>
{
    public CommentsByLessonSpecification(int lessonId)
    {
        Criteria = c =>
            c.LessonId == lessonId &&
            c.ParentCommentId == null &&
            !c.IsDeleted;

        AddInclude(c => c.Author);
        AddInclude("Replies");
        AddInclude("Replies.Author");

        ApplyOrderBy(c => c.CreatedAt);
    }
}

public class CommentsByLessonPagedSpecification : BaseSpecification<LessonComment>
{
    public CommentsByLessonPagedSpecification(
        int lessonId,
        int skip,
        int take)
    {
        Criteria = c =>
            c.LessonId == lessonId &&
            c.ParentCommentId == null &&
            !c.IsDeleted;

        AddInclude(c => c.Author);
        AddInclude("Replies");
        AddInclude("Replies.Author");

        ApplyOrderBy(c => c.CreatedAt);
        ApplyPaging(skip, take);
    }
}

public class CommentByIdSpecification : BaseSpecification<LessonComment>
{
    public CommentByIdSpecification(int commentId)
    {
        Criteria = c => c.Id == commentId;
        AddInclude(c => c.Lesson);
        AddInclude(c => c.ParentComment!);
    }
}

public class CommentsByIdsSpecification : BaseSpecification<LessonComment>
{
    public CommentsByIdsSpecification(int lessonId, IEnumerable<int> commentIds)
    {
        var ids = commentIds.ToList();

        Criteria = c =>
            c.LessonId == lessonId &&
            ids.Contains(c.Id);
    }
}

public class RepliesByParentSpecification : BaseSpecification<LessonComment>
{
    public RepliesByParentSpecification(int parentCommentId)
    {
        Criteria = c => c.ParentCommentId == parentCommentId;
    }
}
