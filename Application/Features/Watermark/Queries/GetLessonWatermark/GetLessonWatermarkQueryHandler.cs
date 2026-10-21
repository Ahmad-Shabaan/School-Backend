using Application.Common;
using Application.Features.Lessons.Errors;
using Application.Features.Watermark.DTOs;
using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using MediatR;

namespace Application.Features.Watermark.Queries.GetLessonWatermark;

public class GetLessonWatermarkQueryHandler
    : IRequestHandler<GetLessonWatermarkQuery, Result<WatermarkDto>>
{
    private readonly ILessonAccessService _lessonAccessService;
    private readonly ICurrentUserService _currentUserService;

    public GetLessonWatermarkQueryHandler(
        ILessonAccessService lessonAccessService,
        ICurrentUserService currentUserService)
    {
        _lessonAccessService = lessonAccessService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<WatermarkDto>> Handle(
        GetLessonWatermarkQuery request,
        CancellationToken cancellationToken)
    {
        var studentId = _currentUserService.UserId;

        var access = await _lessonAccessService.CheckAccessAsync(
            request.LessonId,
            studentId,
            cancellationToken);

        if (!access.IsAccessible)
            return Result<WatermarkDto>.Failed(
                access.MissingRequirement ==
                    LessonAccessRequirements.Enrollment
                    ? "You are not enrolled in this course."
                    : "The lesson is not unlocked yet.",
                access.MissingRequirement ==
                    LessonAccessRequirements.Enrollment
                ? LessonErrors.NotEnrolled
                : LessonErrors.LessonLocked);

        var displayName = _currentUserService.FullName?.Trim();

        if (string.IsNullOrWhiteSpace(displayName))
        {
            displayName = _currentUserService.Email?.Trim() ?? string.Empty;
        }

        return Result<WatermarkDto>.Succeeded(new WatermarkDto
        {
            DisplayName = displayName,
            StudentId = studentId.ToString(),
            MaskedEmail = MaskEmail(_currentUserService.Email)
        });
    }

    internal static string MaskEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return string.Empty;

        var atIndex = email.IndexOf('@');

        if (atIndex <= 0)
            return "***";

        var local = email[..atIndex];
        var domain = email[atIndex..];

        var visible = local.Length <= 2
            ? local[..1]
            : $"{local[0]}***";

        return $"{visible}{domain}";
    }
}
