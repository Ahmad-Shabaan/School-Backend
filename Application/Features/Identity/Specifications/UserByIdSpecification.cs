using Application.Specifications;
using Domain.Entities;

namespace Application.Features.Identity.Specifications;

public class UserByIdSpecification : BaseSpecification<ApplicationUser>
{
    public UserByIdSpecification(Guid userId) => Criteria = u => u.Id == userId;
}
