using Application.Features.Authentication.DTOs;
using Domain.Entities;

namespace Application.Interfaces;

public interface IJwtService
{
    Task<TokenResultDto> GenerateTokensAsync(ApplicationUser user);
}
