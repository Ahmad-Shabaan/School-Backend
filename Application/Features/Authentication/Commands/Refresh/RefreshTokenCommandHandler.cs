using Application.Common;
using Application.Features.Authentication.DTOs;
using Application.Features.Authentication.Errors;
using Application.Features.Authentication.Specifications;
using Application.Interfaces;
using Domain.Entities;
using MediatR;
using System.Data;

namespace Application.Features.Authentication.Commands.Refresh
{
    public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<TokenResultDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJwtService _jwtService;

        public RefreshTokenCommandHandler(IUnitOfWork unitOfWork, IJwtService jwtService)
        {
            _unitOfWork = unitOfWork;
            _jwtService = jwtService;
        }
        public async Task<Result<TokenResultDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            var spec = new RefreshTokenByTokenSpecification(request.RefresherToken);
            var existingRefreshToken = await _unitOfWork.Repository<RefreshToken>().FirstOrDefaultAsync(spec, cancellationToken) ?? throw new Exception("Token not found");

            if (existingRefreshToken.RevokedOn is not null || existingRefreshToken.IsExpired)
                return Result<TokenResultDto>.Failed("Refresh tokes is invalid",AuthErrors.InvalidRefreshToken);
            var tokenResult = await _jwtService.GenerateTokensAsync(existingRefreshToken.User);
            existingRefreshToken.RevokedOn = DateTime.UtcNow;
            var refreshToken = new RefreshToken()
            {
                UserId = existingRefreshToken.User.Id,
                Token = tokenResult.RefreshToken,
                ExpiresOn = tokenResult.RefreshTokenExpiration
            };
            _unitOfWork.Repository<RefreshToken>().Add(refreshToken);
            await _unitOfWork.SaveChangesAsync();
            return Result<TokenResultDto>.Succeeded(tokenResult);
        }
    }
}
