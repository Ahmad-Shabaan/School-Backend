using Application.Features.Courses.Commands.UpdateCourse;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace Application.Features.Courses.Validators;

public class UpdateCourseCommandValidator
    : AbstractValidator<UpdateCourseCommand>
{
    private readonly string[] _allowedExtensions =
    {
        ".pdf",
        ".doc",
        ".png",
        ".jpg",
        ".jpeg"
    };

    public UpdateCourseCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .NotEmpty();

        RuleFor(x => x.Photo)
            .Must(BeValidExtension)
            .When(x => x.Photo is not null)
            .WithMessage("File extension is not allowed.");
    }

    private bool BeValidExtension(IFormFile? file)
    {
        if (file is null)
            return true;

        var extension = Path
            .GetExtension(file.FileName)
            .ToLowerInvariant();

        return _allowedExtensions.Contains(extension);
    }
}