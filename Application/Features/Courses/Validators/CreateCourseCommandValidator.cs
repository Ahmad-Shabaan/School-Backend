using Application.Features.Courses.Commands.CreateCourse;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace Application.Features.Courses.Validators;

public class CreateCourseCommandValidator
    : AbstractValidator<CreateCourseCommand>
{
    private readonly string[] _allowedExtensions =
    {
        ".pdf",
        ".doc",
        ".png",
        ".jpg",
        ".jpeg"
    };

    public CreateCourseCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .NotEmpty();

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Photo)
            .NotNull()
            .Must(BeValidExtension)
            .WithMessage("File extension is not allowed.");
    }

    private bool BeValidExtension(IFormFile? file)
    {
        if (file is null)
            return false;

        var extension = Path
            .GetExtension(file.FileName)
            .ToLowerInvariant();

        return _allowedExtensions.Contains(extension);
    }
}