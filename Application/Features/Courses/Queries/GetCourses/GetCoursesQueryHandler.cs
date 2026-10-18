using Application.Common;
using Application.Features.Courses.DTOs;
using Application.Interfaces;
using MediatR;
namespace Application.Features.Courses.Queries.GetCourses
{
    public class GetCoursesQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetCoursesQuery, Result<IEnumerable<CourseDto>>>
    {
        public async Task<Result<IEnumerable<CourseDto>>> Handle(GetCoursesQuery request, CancellationToken cancellationToken)
        {
            var courses = await unitOfWork.Repository<Domain.Entities.Course>().GetAllAsync(cancellationToken);
            var response = courses.Select(course => new CourseDto
            {
                Id = course.Id,
                CourseName = course.CourseName,
                Description = course.Description,
                File = course.File
            }).ToList();

            
            return Result<IEnumerable<CourseDto>>.Succeeded(response);
        }
    }
}
