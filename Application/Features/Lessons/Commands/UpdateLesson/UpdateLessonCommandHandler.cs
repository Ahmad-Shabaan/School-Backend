using Application.Common;
using Application.Features.Lessons.Commands.CreateLesson;
using Application.Features.Lessons.DTOs;
using Application.Features.Lessons.Errors;
using Application.Features.Lessons.Specifications;
using Application.Interfaces;
using Domain.Entities;
using MediatR;

namespace Application.Features.Lessons.Commands.UpdateLesson;

public class UpdateLessonCommandHandler
    : IRequestHandler<UpdateLessonCommand, Result<LessonDetailsDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public UpdateLessonCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<LessonDetailsDto>> Handle(
        UpdateLessonCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsStaff())
            return Result<LessonDetailsDto>.Failed(
                "You are not authorized to update lessons.",
                LessonErrors.NotAuthorized);

        var lesson = await _unitOfWork
            .Repository<Lesson>()
            .FirstOrDefaultAsync(
                new LessonWithDetailsSpecification(request.Id),
                cancellationToken);

        if (lesson is null)
            return Result<LessonDetailsDto>.Failed(
                "Lesson was not found.",
                LessonErrors.LessonNotFound);

        var hasVideoInput = !string.IsNullOrWhiteSpace(request.YouTubeUrl);

        YouTubeVideoIdExtractor.TryExtract(
            request.YouTubeUrl,
            out var videoId);

        if (hasVideoInput && videoId.Length == 0)
            return Result<LessonDetailsDto>.Failed(
                "The provided YouTube URL or video ID is invalid.",
                LessonErrors.InvalidYouTubeUrl);

        if (!string.IsNullOrWhiteSpace(request.MeetingUrl) &&
            !UrlValidator.IsAllowedUrl(request.MeetingUrl))
            return Result<LessonDetailsDto>.Failed(
                "The provided meeting URL is invalid.",
                LessonErrors.InvalidMeetingUrl);

        lesson.Title = request.Title.Trim();
        lesson.Description = request.Description.Trim();
        lesson.YouTubeVideoId = hasVideoInput ? videoId : null;
        lesson.MeetingUrl = string.IsNullOrWhiteSpace(request.MeetingUrl)
            ? null
            : request.MeetingUrl.Trim();
        lesson.MeetingDateTime = request.MeetingDateTime;

        var goals = request.Goals
            .Where(g => !string.IsNullOrWhiteSpace(g))
            .Select(g => g.Trim())
            .Distinct()
            .ToList();

        var goalRepository = _unitOfWork.Repository<LessonGoal>();

        var existingGoals = lesson.Goals.ToList();

        foreach (var goal in existingGoals.Where(g => !goals.Contains(g.Text)))
        {
            goalRepository.Delete(goal);
            lesson.Goals.Remove(goal);
        }

        foreach (var goalText in goals.Where(
            t => existingGoals.All(g => g.Text != t)))
        {
            var goal = new LessonGoal
            {
                LessonId = lesson.Id,
                Text = goalText
            };

            await goalRepository.AddAsync(goal, cancellationToken);
            lesson.Goals.Add(goal);
        }

        var resourceRepository = _unitOfWork.Repository<LessonResource>();

        var validResources = request.Resources
            .Where(r =>
                !string.IsNullOrWhiteSpace(r.FileUrl) &&
                UrlValidator.IsAllowedUrl(r.FileUrl))
            .ToList();

        foreach (var resource in lesson.Resources.ToList())
        {
            resourceRepository.Delete(resource);
            lesson.Resources.Remove(resource);
        }

        foreach (var resource in validResources)
        {
            var entity = new LessonResource
            {
                LessonId = lesson.Id,
                Title = resource.Title.Trim(),
                FileUrl = resource.FileUrl.Trim(),
                FileType = resource.FileType
            };

            await resourceRepository.AddAsync(entity, cancellationToken);
            lesson.Resources.Add(entity);
        }

        _unitOfWork.Repository<Lesson>().Update(lesson);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<LessonDetailsDto>.Succeeded(
            CreateLessonCommandHandler.MapToDetails(lesson),
            "Lesson updated successfully.");
    }
}
