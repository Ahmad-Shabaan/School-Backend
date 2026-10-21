using Application.Common;
using Application.Features.Lessons.Errors;
using Application.Features.Lessons.Specifications;
using Application.Interfaces;
using Domain.Entities;
using MediatR;

namespace Application.Features.Lessons.Commands.ReorderLessons;

public class ReorderLessonsCommandHandler
    : IRequestHandler<ReorderLessonsCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public ReorderLessonsCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(
        ReorderLessonsCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsStaff())
            return Result.Failed(
                "You are not authorized to reorder lessons.",
                LessonErrors.NotAuthorized);

        var lessons = await _unitOfWork
            .Repository<Lesson>()
            .ListAsync(
                new LessonsByCourseIdSpecification(request.CourseId),
                cancellationToken);

        if (lessons.Count == 0)
            return Result.Failed(
                "Course was not found or has no lessons.",
                LessonErrors.CourseNotFound);

        var currentIds = lessons.Select(l => l.Id).ToHashSet();
        var requestedIds = request.OrderedLessonIds;

        if (requestedIds.Count != lessons.Count ||
            !currentIds.SetEquals(requestedIds) ||
            requestedIds.Count != requestedIds.Distinct().Count())
        {
            return Result.Failed(
                "The reorder payload does not match the lessons of this course.",
                LessonErrors.InvalidReorder);
        }

        var lessonRepository = _unitOfWork.Repository<Lesson>();

        var orderedLessons = requestedIds
            .Select(id => lessons.First(l => l.Id == id))
            .ToList();

        var needsChange = orderedLessons
            .Select((lesson, index) => new { lesson, index })
            .Where(x => x.lesson.Order != x.index)
            .ToList();

        if (needsChange.Count == 0)
            return Result.Succeeded("Lessons reordered successfully.");

        foreach (var item in needsChange)
        {
            item.lesson.Order = -1000000 + item.index;
            lessonRepository.Update(item.lesson);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (var item in needsChange)
        {
            item.lesson.Order = item.index;
            lessonRepository.Update(item.lesson);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Succeeded("Lessons reordered successfully.");
    }
}
