using Application.Specifications;
using Domain.Entities;

namespace Application.Features.Authentication.Specifications;

public class UserByEmailSpecification : BaseSpecification<ApplicationUser>
{
    public UserByEmailSpecification(string email)
    {
        Criteria = u => u.Email == email;
        AddInclude(u => u.RefreshTokens);
    }
}
