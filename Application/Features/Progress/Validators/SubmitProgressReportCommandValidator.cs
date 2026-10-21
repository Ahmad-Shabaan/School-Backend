using Application.Features.Progress.Commands.SubmitProgressReport;
using FluentValidation;

namespace Application.Features.Progress.Validators;

public class SubmitProgressReportCommandValidator
    : AbstractValidator<SubmitProgressReportCommand>
{
    public SubmitProgressReportCommandValidator()
    {
        RuleFor(x => x.LessonId)
            .GreaterThan(0);

        RuleFor(x => x.CurrentTimeSeconds)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.DurationSeconds)
            .GreaterThan(0);

        RuleFor(x => x)
            .Must(x =>
                (x.WatchedSegmentStartSeconds.HasValue &&
                 x.WatchedSegmentEndSeconds.HasValue) ||
                (!x.WatchedSegmentStartSeconds.HasValue &&
                 !x.WatchedSegmentEndSeconds.HasValue))
            .WithMessage(
                "Watch segment start and end must be provided together.");

        RuleFor(x => x.WatchedSegmentStartSeconds)
            .GreaterThanOrEqualTo(0)
            .When(x => x.WatchedSegmentStartSeconds.HasValue);

        RuleFor(x => x.WatchedSegmentEndSeconds)
            .GreaterThan(0)
            .When(x => x.WatchedSegmentEndSeconds.HasValue);
    }
}
