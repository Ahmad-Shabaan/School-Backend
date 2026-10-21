using Application.Features.Lessons.Commands.ReorderLessons;
using Application.Features.Lessons.Commands.UpdateLesson;
using FluentValidation;

namespace Application.Features.Lessons.Validators;

public class UpdateLessonCommandValidator
    : AbstractValidator<UpdateLessonCommand>
{
    public UpdateLessonCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);

        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(2000);

        RuleFor(x => x.YouTubeUrl)
            .MaximumLength(500)
            .When(x => x.YouTubeUrl is not null);

        RuleFor(x => x.MeetingUrl)
            .MaximumLength(1000)
            .When(x => x.MeetingUrl is not null);

        RuleForEach(x => x.Goals)
            .NotEmpty()
            .MaximumLength(1000);

        RuleForEach(x => x.Resources)
            .ChildRules(resource =>
            {
                resource.RuleFor(r => r.Title)
                    .NotEmpty()
                    .MaximumLength(500);

                resource.RuleFor(r => r.FileUrl)
                    .NotEmpty()
                    .MaximumLength(2000);
            });
    }
}

public class ReorderLessonsCommandValidator
    : AbstractValidator<ReorderLessonsCommand>
{
    public ReorderLessonsCommandValidator()
    {
        RuleFor(x => x.CourseId)
            .GreaterThan(0);

        RuleFor(x => x.OrderedLessonIds)
            .NotEmpty();

        RuleForEach(x => x.OrderedLessonIds)
            .GreaterThan(0);
    }
}
