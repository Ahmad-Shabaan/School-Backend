using Application.Specifications;
using Domain.Entities;
namespace Application.Features.Authentication.Specifications
{
    public class RefreshTokenByTokenSpecification : BaseSpecification<RefreshToken>
    {
        public RefreshTokenByTokenSpecification(string token)
        {
            Criteria = t => t.Token == token;
            AddInclude(t => t.User);
        }

    }
}
