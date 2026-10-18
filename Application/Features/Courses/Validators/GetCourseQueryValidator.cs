using Application.Features.Courses.Queries.GetCourse;
using FluentValidation;

namespace Application.Features.Courses.Validators;

public class GetCourseQueryValidator
    : AbstractValidator<GetCourseQuery>
{
    public GetCourseQueryValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);
    }
}