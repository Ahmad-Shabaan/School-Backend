using Application.Common;
using Application.Features.Courses.DTOs;
using Application.Features.Courses.Errors;
using Application.Features.Courses.Specifications;
using Application.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Courses.Commands.CreateCourse;

public class CreateCourseCommandHandler
    : IRequestHandler<CreateCourseCommand, Result<CourseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<ApplicationUser> _userManager;

    public CreateCourseCommandHandler(
        IUnitOfWork unitOfWork,
        UserManager<ApplicationUser> userManager)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
    }

    public async Task<Result<CourseDto>> Handle(
        CreateCourseCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user is null)
            return Result<CourseDto>.Failed("User not found.", CourseErrors.UserNotFound);

        if (!user.IsActive)
            return Result<CourseDto>.Failed(
                "Your account has been deactivated.",
                CourseErrors.InactiveUser);

        if (request.Photo is null)
            return Result<CourseDto>.Failed(
                "Course file is required.",
                CourseErrors.FileRequired);

        using var dataStream = new MemoryStream();

        await request.Photo.CopyToAsync(
            dataStream,
            cancellationToken);

        var course = new Course
        {
            CourseName = request.Name,
            Description = request.Description,
            File = dataStream.ToArray()
        };

        await _unitOfWork
            .Repository<Course>()
            .AddAsync(course, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var userCourse = new UserCourse
        {
            ApplicationUserId = user.Id,
            CourseId = course.Id
        };

        await _unitOfWork
            .Repository<UserCourse>()
            .AddAsync(userCourse, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new CourseDto
        {
            Id = course.Id,
            CourseName = course.CourseName,
            Description = course.Description,
            File = course.File
        };

        return Result<CourseDto>.Succeeded(
            response,
            "Course created successfully.");
    }
}