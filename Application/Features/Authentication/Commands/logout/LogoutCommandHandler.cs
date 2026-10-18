using Application.Common;
using Application.Features.Authentication.Specifications;
using Application.Interfaces;
using Domain.Entities;
using MediatR;
namespace Application.Features.Authentication.Commands.logout
{
    public class LogoutCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<LogoutCommand, Result>
    {
        public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
         
            var spec = new RefreshTokenByTokenSpecification(request.RefreshToken);
            var refreshToken = await unitOfWork.Repository<RefreshToken>().GetAsync(spec, cancellationToken);
            if (refreshToken is null)
                return Result.Failed("Invalid token");

            refreshToken.RevokedOn = DateTime.UtcNow;
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Succeeded("Succeeded");
        }
    }
}
