using Application.Common;
using Application.Features.Lessons.DTOs;
using Application.Features.Lessons.Errors;
using Application.Features.Lessons.Specifications;
using Application.Interfaces;
using Domain.Entities;
using MediatR;

namespace Application.Features.Lessons.Commands.CreateLesson;

public class CreateLessonCommandHandler
    : IRequestHandler<CreateLessonCommand, Result<LessonDetailsDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CreateLessonCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<LessonDetailsDto>> Handle(
        CreateLessonCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsStaff())
            return Result<LessonDetailsDto>.Failed(
                "You are not authorized to create lessons.",
                LessonErrors.NotAuthorized);

        var course = await _unitOfWork
            .Repository<Course>()
            .GetByIdAsync(request.CourseId, cancellationToken);

        if (course is null)
            return Result<LessonDetailsDto>.Failed(
                "Course was not found.",
                LessonErrors.CourseNotFound);

        var hasVideoInput = !string.IsNullOrWhiteSpace(request.YouTubeUrl);

        YouTubeVideoIdExtractor.TryExtract(
            request.YouTubeUrl,
            out var videoId);

        if (hasVideoInput && videoId.Length == 0)
        {
            return Result<LessonDetailsDto>.Failed(
                "The provided YouTube URL or video ID is invalid.",
                LessonErrors.InvalidYouTubeUrl);
        }

        if (!string.IsNullOrWhiteSpace(request.MeetingUrl) &&
            !UrlValidator.IsAllowedUrl(request.MeetingUrl))
        {
            return Result<LessonDetailsDto>.Failed(
                "The provided meeting URL is invalid.",
                LessonErrors.InvalidMeetingUrl);
        }

        var existingLessons = await _unitOfWork
            .Repository<Lesson>()
            .ListAsync(
                new LessonsByCourseIdSpecification(request.CourseId),
                cancellationToken);

        var nextOrder = existingLessons.Count == 0
            ? 1
            : existingLessons.Max(l => l.Order) + 1;

        var lesson = new Lesson
        {
            CourseId = request.CourseId,
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Order = nextOrder,
            YouTubeVideoId = string.IsNullOrWhiteSpace(videoId)
                ? null
                : videoId,
            MeetingUrl = string.IsNullOrWhiteSpace(request.MeetingUrl)
                ? null
                : request.MeetingUrl.Trim(),
            MeetingDateTime = request.MeetingDateTime
        };

        foreach (var goalText in request.Goals.Where(
            g => !string.IsNullOrWhiteSpace(g)))
        {
            lesson.Goals.Add(new LessonGoal
            {
                Text = goalText.Trim()
            });
        }

        foreach (var resource in request.Resources)
        {
            if (string.IsNullOrWhiteSpace(resource.FileUrl) ||
                !UrlValidator.IsAllowedUrl(resource.FileUrl))
            {
                continue;
            }

            lesson.Resources.Add(new LessonResource
            {
                Title = resource.Title.Trim(),
                FileUrl = resource.FileUrl.Trim(),
                FileType = resource.FileType
            });
        }

        await _unitOfWork
            .Repository<Lesson>()
            .AddAsync(lesson, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<LessonDetailsDto>.Succeeded(
            MapToDetails(lesson),
            "Lesson created successfully.");
    }

    internal static LessonDetailsDto MapToDetails(Lesson lesson)
    {
        return new LessonDetailsDto
        {
            Id = lesson.Id,
            CourseId = lesson.CourseId,
            Title = lesson.Title,
            Description = lesson.Description,
            Order = lesson.Order,
            YouTubeVideoId = lesson.YouTubeVideoId,
            MeetingUrl = lesson.MeetingUrl,
            MeetingDateTime = lesson.MeetingDateTime,
            Goals = lesson.Goals
                .Select(g => new LessonGoalDto
                {
                    Id = g.Id,
                    Text = g.Text
                })
                .ToList(),
            Resources = lesson.Resources
                .Select(r => new LessonResourceDto
                {
                    Id = r.Id,
                    Title = r.Title,
                    FileUrl = r.FileUrl,
                    FileType = r.FileType
                })
                .ToList(),
            HasQuiz = lesson.Quiz is not null,
            QuizId = lesson.Quiz?.Id
        };
    }
}
