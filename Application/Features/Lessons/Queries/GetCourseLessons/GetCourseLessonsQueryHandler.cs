using Application.Common;
using Application.Features.Lessons.DTOs;
using Application.Features.Lessons.Errors;
using Application.Features.Lessons.Specifications;
using Application.Features.Progress.Specifications;
using Application.Features.Quizzes.Specifications;
using Application.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Options;

namespace Application.Features.Lessons.Queries.GetCourseLessons;

public class GetCourseLessonsQueryHandler
    : IRequestHandler<GetCourseLessonsQuery, Result<List<LessonStatusDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly LearningPlatformOptions _options;

    public GetCourseLessonsQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IOptions<LearningPlatformOptions> options)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _options = options.Value;
    }

    public async Task<Result<List<LessonStatusDto>>> Handle(
        GetCourseLessonsQuery request,
        CancellationToken cancellationToken)
    {
        var course = await _unitOfWork
            .Repository<Course>()
            .GetByIdAsync(request.CourseId, cancellationToken);

        if (course is null)
            return Result<List<LessonStatusDto>>.Failed(
                "Course was not found.",
                LessonErrors.CourseNotFound);

        var studentId = _currentUserService.UserId;
        var isStaff = _currentUserService.IsStaff();

        if (!isStaff)
        {
            var enrollment = await _unitOfWork
                .Repository<UserCourse>()
                .FirstOrDefaultAsync(
                    new UserCourseByStudentAndCourseSpecification(
                        studentId,
                        request.CourseId),
                    cancellationToken);

            if (enrollment is null)
                return Result<List<LessonStatusDto>>.Failed(
                    "You are not enrolled in this course.",
                    LessonErrors.NotEnrolled);
        }

        var lessons = await _unitOfWork
            .Repository<Lesson>()
            .ListAsync(
                new LessonsByCourseIdSpecification(request.CourseId),
                cancellationToken);

        var progresses = await _unitOfWork
            .Repository<Domain.Entities.LessonProgress>()
            .ListAsync(
                new LessonProgressByStudentAndCourseSpecification(
                    studentId,
                    request.CourseId),
                cancellationToken);

        var quizzes = await _unitOfWork
            .Repository<Quiz>()
            .ListAsync(
                new QuizzesByCourseSpecification(request.CourseId),
                cancellationToken);

        var attempts = await _unitOfWork
            .Repository<QuizAttempt>()
            .ListAsync(
                new QuizAttemptsByStudentAndCourseSpecification(
                    studentId,
                    request.CourseId),
                cancellationToken);

        var progressByLesson = progresses
            .ToDictionary(p => p.LessonId, p => p);

        var quizByLesson = quizzes
            .ToDictionary(q => q.LessonId, q => q);

        var passedQuizIds = quizzes
            .Where(q => attempts.Any(a =>
                a.QuizId == q.Id &&
                a.IsPassed &&
                a.Score >= q.PassingScorePercent))
            .Select(q => q.Id)
            .ToHashSet();

        var statuses = new List<LessonStatusDto>();

        for (var i = 0; i < lessons.Count; i++)
        {
            var lesson = lessons[i];

            var hasVideo = !string.IsNullOrWhiteSpace(lesson.YouTubeVideoId);

            var hasQuiz = quizByLesson.TryGetValue(lesson.Id, out var quiz);

            var videoCompleted = !hasVideo ||
                (progressByLesson.TryGetValue(lesson.Id, out var progress) &&
                 progress.DurationSeconds > 0 &&
                 progress.WatchedPercentage >=
                     _options.VideoCompletionThresholdPercent);

            var quizPassed = hasQuiz && passedQuizIds.Contains(quiz!.Id);

            bool isUnlocked;

            if (isStaff || i == 0)
            {
                isUnlocked = true;
            }
            else
            {
                var previous = lessons[i - 1];

                var previousVideoCompleted =
                    IsVideoCompleted(previous, progressByLesson);

                var previousQuizPassed = IsQuizPassed(
                    previous,
                    quizByLesson,
                    passedQuizIds);

                isUnlocked =
                    previousVideoCompleted && previousQuizPassed;
            }

            statuses.Add(new LessonStatusDto
            {
                Id = lesson.Id,
                Order = lesson.Order,
                Title = lesson.Title,
                IsUnlocked = isUnlocked,
                MeetingUrl = lesson.MeetingUrl,
                MeetingDateTime = lesson.MeetingDateTime,
                WatchedPercentage = progressByLesson
                    .TryGetValue(lesson.Id, out var ownProgress)
                    ? ownProgress.WatchedPercentage
                    : 0m,
                IsVideoCompleted = videoCompleted,
                IsQuizPassed = quizPassed,
                HasQuiz = hasQuiz
            });
        }

        return Result<List<LessonStatusDto>>.Succeeded(statuses);
    }

    private bool IsVideoCompleted(
        Lesson lesson,
        Dictionary<int, Domain.Entities.LessonProgress> progressByLesson)
    {
        if (string.IsNullOrWhiteSpace(lesson.YouTubeVideoId))
            return true;

        return progressByLesson.TryGetValue(lesson.Id, out var progress) &&
               progress.DurationSeconds > 0 &&
               progress.WatchedPercentage >=
                   _options.VideoCompletionThresholdPercent;
    }

    private bool IsQuizPassed(
        Lesson lesson,
        Dictionary<int, Quiz> quizByLesson,
        HashSet<int> passedQuizIds)
    {
        if (!quizByLesson.TryGetValue(lesson.Id, out var quiz))
            return true;

        return passedQuizIds.Contains(quiz.Id);
    }
}
