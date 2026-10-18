

using Application.Features.Identity.DTOs;
using Application.Specifications;
using Domain.Entities;

namespace Application.Features.Identity.Specifications
{
    public class UsersEmailProjectionSpecification : BaseProjectionSpecification<ApplicationUser, UserEmailDto>
    {
        public UsersEmailProjectionSpecification(string AdminEmail)
            : base(u => new UserEmailDto(u.Id, u.Email ?? string.Empty))
        {
            Criteria = u => u.Email != AdminEmail;
        }
    }


}
