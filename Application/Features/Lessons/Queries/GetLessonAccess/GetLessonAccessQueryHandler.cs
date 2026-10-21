using Application.Common;
using Application.Features.Lessons.DTOs;
using Application.Features.Lessons.Errors;
using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using MediatR;

namespace Application.Features.Lessons.Queries.GetLessonAccess;

public class GetLessonAccessQueryHandler
    : IRequestHandler<GetLessonAccessQuery, Result<LessonAccessDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILessonAccessService _lessonAccessService;
    private readonly ICurrentUserService _currentUserService;

    public GetLessonAccessQueryHandler(
        IUnitOfWork unitOfWork,
        ILessonAccessService lessonAccessService,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _lessonAccessService = lessonAccessService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<LessonAccessDto>> Handle(
        GetLessonAccessQuery request,
        CancellationToken cancellationToken)
    {
        var lesson = await _unitOfWork
            .Repository<Lesson>()
            .GetByIdAsync(request.LessonId, cancellationToken);

        if (lesson is null)
            return Result<LessonAccessDto>.Failed(
                "Lesson was not found.",
                LessonErrors.LessonNotFound);

        var access = await _lessonAccessService.CheckAccessAsync(
            request.LessonId,
            _currentUserService.UserId,
            cancellationToken);

        var response = new LessonAccessDto
        {
            IsAccessible = access.IsAccessible,
            IsEnrolled = access.IsEnrolled,
            IsVideoCompleted = access.IsVideoCompleted,
            IsQuizPassed = access.IsQuizPassed,
            MissingRequirement = access.MissingRequirement
        };

        return Result<LessonAccessDto>.Succeeded(response);
    }
}
