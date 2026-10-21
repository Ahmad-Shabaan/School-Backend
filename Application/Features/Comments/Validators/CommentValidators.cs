using Application.Features.Comments.Commands.CreateComment;
using Application.Features.Comments.Commands.UpdateComment;
using FluentValidation;

namespace Application.Features.Comments.Validators;

public class CreateCommentCommandValidator
    : AbstractValidator<CreateCommentCommand>
{
    public CreateCommentCommandValidator()
    {
        RuleFor(x => x.LessonId)
            .GreaterThan(0);

        RuleFor(x => x.Content)
            .NotEmpty()
            .MaximumLength(2000);

        RuleFor(x => x.ParentCommentId)
            .GreaterThan(0)
            .When(x => x.ParentCommentId.HasValue);
    }
}

public class UpdateCommentCommandValidator
    : AbstractValidator<UpdateCommentCommand>
{
    public UpdateCommentCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);

        RuleFor(x => x.Content)
            .NotEmpty()
            .MaximumLength(2000);
    }
}
