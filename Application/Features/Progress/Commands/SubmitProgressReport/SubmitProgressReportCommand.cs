using Application.Common;
using Application.Features.Progress.DTOs;
using MediatR;

namespace Application.Features.Progress.Commands.SubmitProgressReport;

public class SubmitProgressReportCommand : IRequest<Result<LessonProgressDto>>
{
    public int LessonId { get; set; }

    public double CurrentTimeSeconds { get; set; }

    public double DurationSeconds { get; set; }

    public double? WatchedSegmentStartSeconds { get; set; }

    public double? WatchedSegmentEndSeconds { get; set; }
}
