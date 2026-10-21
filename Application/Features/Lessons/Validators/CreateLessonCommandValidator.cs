using Application.Features.Lessons.Commands.CreateLesson;
using FluentValidation;

namespace Application.Features.Lessons.Validators;

public class CreateLessonCommandValidator
    : AbstractValidator<CreateLessonCommand>
{
    public CreateLessonCommandValidator()
    {
        RuleFor(x => x.CourseId)
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
