using Application.Features.Quizzes.Commands.CreateQuiz;
using Application.Features.Quizzes.Commands.UpdateQuiz;
using Application.Features.Quizzes.DTOs;
using FluentValidation;

namespace Application.Features.Quizzes.Validators;

public static class QuizPayloadRules
{
    public static void Apply<T>(AbstractValidator<T> validator)
        where T : IQuizPayload
    {
        validator.RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(500);

        validator.RuleFor(x => x.PassingScorePercent)
            .InclusiveBetween(0m, 100m);

        validator.RuleFor(x => x.Questions)
            .NotEmpty();

        validator.RuleForEach(x => x.Questions).ChildRules(question =>
        {
            question.RuleFor(q => q.Text)
                .NotEmpty()
                .MaximumLength(2000);

            question.RuleFor(q => q.Options)
                .Must(options => options is { Count: >= 2 })
                .WithMessage("Each question must have at least two answer options.");

            question.RuleFor(q => q.Options)
                .Must(options =>
                    options is not null &&
                    options.Count(o => o.IsCorrect) == 1)
                .WithMessage("Each question must have exactly one correct answer.");

            question.RuleForEach(q => q.Options).ChildRules(option =>
            {
                option.RuleFor(o => o.Text)
                    .NotEmpty()
                    .MaximumLength(1000);
            });
        });
    }
}

public class CreateQuizCommandValidator
    : AbstractValidator<CreateQuizCommand>
{
    public CreateQuizCommandValidator()
    {
        QuizPayloadRules.Apply(this);
    }
}

public class UpdateQuizCommandValidator
    : AbstractValidator<UpdateQuizCommand>
{
    public UpdateQuizCommandValidator()
    {
        QuizPayloadRules.Apply(this);
    }
}
