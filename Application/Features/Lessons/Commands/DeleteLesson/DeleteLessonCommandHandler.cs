using Application.Common;
using Application.Features.Lessons.Errors;
using Application.Features.Lessons.Specifications;
using Application.Interfaces;
using Domain.Entities;
using MediatR;

namespace Application.Features.Lessons.Commands.DeleteLesson;

public class DeleteLessonCommandHandler
    : IRequestHandler<DeleteLessonCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public DeleteLessonCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(
        DeleteLessonCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsStaff())
            return Result.Failed(
                "You are not authorized to delete lessons.",
                LessonErrors.NotAuthorized);

        var lesson = await _unitOfWork
            .Repository<Lesson>()
            .FirstOrDefaultAsync(
                new LessonByIdSpecification(request.Id),
                cancellationToken);

        if (lesson is null)
            return Result.Failed(
                "Lesson was not found.",
                LessonErrors.LessonNotFound);

        var courseId = lesson.CourseId;

        _unitOfWork
            .Repository<Lesson>()
            .Delete(lesson);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await RenumberLessonsAsync(courseId, cancellationToken);

        return Result.Succeeded("Lesson deleted successfully.");
    }

    private async Task RenumberLessonsAsync(
        int courseId,
        CancellationToken cancellationToken)
    {
        var lessons = await _unitOfWork
            .Repository<Lesson>()
            .ListAsync(
                new LessonsByCourseIdSpecification(courseId),
                cancellationToken);

        var lessonRepository = _unitOfWork.Repository<Lesson>();

        var needsRenumbering = lessons
            .Select((lesson, index) => new { lesson, index })
            .Where(x => x.lesson.Order != x.index)
            .ToList();

        if (needsRenumbering.Count == 0)
            return;

        foreach (var item in needsRenumbering)
        {
            item.lesson.Order = -1000000 + item.index;
            lessonRepository.Update(item.lesson);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (var item in needsRenumbering)
        {
            item.lesson.Order = item.index;
            lessonRepository.Update(item.lesson);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
