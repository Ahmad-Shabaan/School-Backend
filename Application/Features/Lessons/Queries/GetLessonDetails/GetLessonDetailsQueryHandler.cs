using Application.Common;
using Application.Features.Lessons.Commands.CreateLesson;
using Application.Features.Lessons.DTOs;
using Application.Features.Lessons.Errors;
using Application.Features.Lessons.Specifications;
using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using MediatR;

namespace Application.Features.Lessons.Queries.GetLessonDetails;

public class GetLessonDetailsQueryHandler
    : IRequestHandler<GetLessonDetailsQuery, Result<LessonDetailsDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILessonAccessService _lessonAccessService;
    private readonly ICurrentUserService _currentUserService;

    public GetLessonDetailsQueryHandler(
        IUnitOfWork unitOfWork,
        ILessonAccessService lessonAccessService,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _lessonAccessService = lessonAccessService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<LessonDetailsDto>> Handle(
        GetLessonDetailsQuery request,
        CancellationToken cancellationToken)
    {
        var lesson = await _unitOfWork
            .Repository<Lesson>()
            .FirstOrDefaultAsync(
                new LessonWithDetailsSpecification(request.LessonId),
                cancellationToken);

        if (lesson is null)
            return Result<LessonDetailsDto>.Failed(
                "Lesson was not found.",
                LessonErrors.LessonNotFound);

        var access = await _lessonAccessService.CheckAccessAsync(
            request.LessonId,
            _currentUserService.UserId,
            cancellationToken);

        if (!access.IsAccessible)
            return Result<LessonDetailsDto>.Failed(
                access.MissingRequirement ==
                    LessonAccessRequirements.Enrollment
                    ? "You are not enrolled in this course."
                    : "The lesson is not unlocked yet.",
                access.MissingRequirement ==
                    LessonAccessRequirements.Enrollment
                ? LessonErrors.NotEnrolled
                : LessonErrors.LessonLocked);

        var response = CreateLessonCommandHandler.MapToDetails(lesson);

        response.Access = new LessonAccessDto
        {
            IsAccessible = access.IsAccessible,
            IsEnrolled = access.IsEnrolled,
            IsVideoCompleted = access.IsVideoCompleted,
            IsQuizPassed = access.IsQuizPassed,
            MissingRequirement = access.MissingRequirement
        };

        return Result<LessonDetailsDto>.Succeeded(response);
    }
}
