using Application.Common;
using Application.Features.Courses.DTOs;
using Application.Features.Courses.Errors;
using Application.Features.Courses.Specifications;
using Application.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Courses.Queries.GetUserCourses;

public class GetUserCoursesQueryHandler
    : IRequestHandler<GetUserCoursesQuery, Result<List<CourseDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<ApplicationUser> _userManager;

    public GetUserCoursesQueryHandler(
        IUnitOfWork unitOfWork,
        UserManager<ApplicationUser> userManager)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
    }

    public async Task<Result<List<CourseDto>>> Handle(
        GetUserCoursesQuery request,
        CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user is null)
            return Result<List<CourseDto>>.Failed(
                "User not found.",
                CourseErrors.UserNotFound);

        var specification =
            new UserCoursesSpecification(user.Id);

        var userCourses = await _unitOfWork
            .Repository<UserCourse>()
            .ListAsync(
                specification,
                cancellationToken);

        var response = userCourses
            .Select(x => new CourseDto
            {
                Id = x.Course.Id,
                CourseName = x.Course.CourseName,
                Description = x.Course.Description,
                File = x.Course.File
            })
            .ToList();

        return Result<List<CourseDto>>.Succeeded(response);
    }
}