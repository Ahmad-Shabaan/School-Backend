using Application.Features.Quizzes.Commands.SubmitQuizAttempt;
using FluentValidation;

namespace Application.Features.Quizzes.Validators;

public class SubmitQuizAttemptCommandValidator
    : AbstractValidator<SubmitQuizAttemptCommand>
{
    public SubmitQuizAttemptCommandValidator()
    {
        RuleFor(x => x.LessonId)
            .GreaterThan(0);

        RuleFor(x => x.Answers)
            .NotEmpty();

        RuleForEach(x => x.Answers)
            .ChildRules(answer =>
            {
                answer.RuleFor(a => a.QuestionId)
                    .GreaterThan(0);

                answer.RuleFor(a => a.OptionId)
                    .GreaterThan(0);
            });
    }
}
