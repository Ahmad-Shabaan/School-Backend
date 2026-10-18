using Application.Specifications;
using Domain.Entities;

namespace Application.Features.Identity.Specifications;

public class UserByEmailSpecification : BaseSpecification<ApplicationUser>
{
    public UserByEmailSpecification(string email)
    {
        Criteria = u => u.Email == email;
    }
}
