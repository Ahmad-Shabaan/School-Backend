using Application.Common;
using Application.Features.Identity.DTOs;
using Application.Features.Identity.Errors;
using Application.Features.Identity.Specifications;
using Application.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System.Net;

namespace Application.Features.Identity.Commands.CreateUser;

public class CreateUserCommandHandler
    : IRequestHandler<CreateUserCommand, Result<UserDetailDto>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<AppRoles> _roleManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISettings _settings;
    public CreateUserCommandHandler(
        UserManager<ApplicationUser> userManager,
        RoleManager<AppRoles> roleManager,
        IUnitOfWork unitOfWork,
        ISettings settings)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _unitOfWork = unitOfWork;
        _settings = settings;

    }

    public async Task<Result<UserDetailDto>> Handle(
        CreateUserCommand request,
        CancellationToken cancellationToken)
    {
        foreach (var role in request.Roles)
        {
            if (!await _roleManager.RoleExistsAsync(role))
                return Result<UserDetailDto>.Failed($"Processing of creating user is failed.", UserErrors.RoleNotFount);
        }

        var existing = await _unitOfWork.Repository<ApplicationUser>()
            .FirstOrDefaultAsync(
                new UserByEmailSpecification(request.Email),
                cancellationToken);

        if (existing is not null)
            return Result<UserDetailDto>.Failed("A user with this email already exists.", UserErrors.AlreadyExists);

        var user = new ApplicationUser
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            UserName = request.Email,
            IsActive = request.IsActive,
            ForceChangedPassword = request.ForceChangePassword,
            EmailConfirmed = true
        };

        var createResult = await _userManager.CreateAsync(
            user,
            _settings.DefaultPassword);

        if (!createResult.Succeeded)
        {
            var errors = createResult.Errors.Select(e => new Error(e.Code, e.Description, HttpStatusCode.BadRequest));
            return Result<UserDetailDto>.Failed($"Failed to create user", null, errors);
        }

        if (request.Roles.Count > 0)
        {
            var roleResult = await _userManager.AddToRolesAsync(user, request.Roles);

            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);
                var errors = roleResult.Errors.Select(e => new Error(e.Code, e.Description, HttpStatusCode.BadRequest));
                return Result<UserDetailDto>.Failed("Failed to assign roles.",null, errors);
            }
        }

        var spec = new UserDetailProjectionSpecification(user.Id);

        var userDto = await _unitOfWork.Repository<ApplicationUser>()
            .FirstOrDefaultAsync(spec, cancellationToken);

        if (userDto is null)
            return Result<UserDetailDto>.Failed("User was created but could not be retrieved.", UserErrors.UserNotFound);

        userDto.Roles = await _userManager.GetRolesAsync(user);

        return Result<UserDetailDto>.Succeeded(userDto, "User created successfully.");
    }
}
