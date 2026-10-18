using Application.Features.Courses.Queries.GetUserCourses;
using FluentValidation;

namespace Application.Features.Courses.Validators;

public class GetUserCoursesQueryValidator
    : AbstractValidator<GetUserCoursesQuery>
{
    public GetUserCoursesQueryValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();
    }
}