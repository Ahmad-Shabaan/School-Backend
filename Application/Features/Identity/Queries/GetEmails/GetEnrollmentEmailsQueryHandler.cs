using Application.Common;
using Application.Features.Enrollments.DTOs;
using Application.Features.Identity.DTOs;
using Application.Features.Identity.Specifications;
using Application.Interfaces;
using Domain.Entities;
using MediatR;


namespace Application.Features.Identity.Queries.GetEmails
{
    public class GetEnrollmentEmailsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService) : IRequestHandler<GetEnrollmentEmailsQuery, Result<IReadOnlyList<EnrollmentEmailsDto>>>
    {
        public async Task<Result<IReadOnlyList<EnrollmentEmailsDto>>> Handle(GetEnrollmentEmailsQuery request, CancellationToken cancellationToken)
        {
            if (currentUserService.Email is null)
            {
                return Result<IReadOnlyList<EnrollmentEmailsDto>>.Failed("Current user email is null.");
            }
            if(request.CourseId <= 0)
                return Result<IReadOnlyList<EnrollmentEmailsDto>>.Failed("Invalid course ID.");
            var spec = new UsersEmailProjectionSpecification(currentUserService.Email);
            var userEmails = await unitOfWork.Repository<ApplicationUser>().ListAsync(spec, cancellationToken);
            var enrollmentSpec = new UsersIdByCoureseIdProjectionSpecification(request.CourseId);
            var enromentUsersIds = await unitOfWork.Repository<UserCourse>().ListAsync(enrollmentSpec, cancellationToken);
            var ids = enromentUsersIds.ToHashSet();

            var response = userEmails.Select(userEmails => new EnrollmentEmailsDto(ids.Contains(userEmails.UserId.ToString().ToUpper()), userEmails.Email)).ToList();
            return Result<IReadOnlyList<EnrollmentEmailsDto>>.Succeeded(response);
        }
    }
}
