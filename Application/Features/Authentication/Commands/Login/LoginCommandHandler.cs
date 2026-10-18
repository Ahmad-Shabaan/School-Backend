using Application.Common;
using Application.Features.Authentication.DTOs;
using Application.Features.Authentication.Errors;
using Application.Features.Authentication.Specifications;
using Application.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Authentication.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthResponseDto>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtService _jwtService;
    private readonly IUnitOfWork _unitOfWork;

    public LoginCommandHandler(
        UserManager<ApplicationUser> userManager,
        IJwtService jwtService,
        IUnitOfWork unitOfWork)
    {
        _userManager = userManager;
        _jwtService = jwtService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AuthResponseDto>> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        var specification = new UserByEmailSpecification(request.Email);

        var user = await _unitOfWork.Repository<ApplicationUser>()
            .FirstOrDefaultAsync(specification, cancellationToken);

        if (user is null)
            return Result<AuthResponseDto>.Failed("Invalid email or password.", AuthErrors.InvalidCredentials);

        if (!user.IsActive)
            return Result<AuthResponseDto>.Failed("Your account has been deactivated.", AuthErrors.AccessDenied);

        var validPassword = await _userManager.CheckPasswordAsync(
            user,
            request.Password);

        if (!validPassword)
            return Result<AuthResponseDto>.Failed("Invalid email or password.", AuthErrors.InvalidCredentials);

        var tokenResult = await _jwtService.GenerateTokensAsync(user);

        _unitOfWork.Repository<RefreshToken>().Add(new RefreshToken
        {
            UserId = user.Id,
            Token = tokenResult.RefreshToken,
            ExpiresOn = tokenResult.RefreshTokenExpiration,
            CreatedOn = DateTime.UtcNow
        });

        user.LastLogin = DateTime.UtcNow;
        _unitOfWork.Repository<ApplicationUser>().Update(user);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var roles = await _userManager.GetRolesAsync(user);

        var response = new AuthResponseDto
        {
            Success = true,
            Message = "Login successful.",
            AccessToken = tokenResult.AccessToken,
            RefreshToken = tokenResult.RefreshToken,
            ExpiresAt = tokenResult.AccessTokenExpiration,
            ForceChangePassword = user.ForceChangedPassword,
            User = new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email!,
                Roles = roles
            }
        };

        return Result<AuthResponseDto>.Succeeded(response, "Login successful.");
    }
}
