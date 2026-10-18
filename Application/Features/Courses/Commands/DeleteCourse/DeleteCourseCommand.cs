using Application.Common;
using Application.Features.Courses.DTOs;
using MediatR;

namespace Application.Features.Courses.Commands.DeleteCourse;

public class DeleteCourseCommand : IRequest<Result<bool>>
{
    public int Id { get; set; }

    public DeleteCourseCommand(int id)
    {
        Id = id;
    }
}