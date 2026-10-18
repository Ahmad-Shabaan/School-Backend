using Application.Features.Identity.DTOs;
using Application.Specifications;
using Domain.Entities;

namespace Application.Features.Identity.Specifications;

public class UserDetailProjectionSpecification
    : BaseProjectionSpecification<ApplicationUser, UserDetailDto>
{
    public UserDetailProjectionSpecification()
        : base(u => new UserDetailDto
        {
            Id = u.Id,
            FirstName = u.FirstName,
            LastName = u.LastName,
            Email = u.Email ?? string.Empty,
            UserName = u.UserName ?? string.Empty,
            IsActive = u.IsActive,
            ForceChangePassword = u.ForceChangedPassword,
            CreatedAt = u.CreatedAt,
            UpdatedAt = u.UpdateAt,
            LastLogin = u.LastLogin,
            Roles = new List<string>()
        })
    {
        ApplyOrderByDescending(u => u.CreatedAt);
    }

    public UserDetailProjectionSpecification(Guid userId)
        : this()
    {
        Criteria = u => u.Id == userId;
    }

    public UserDetailProjectionSpecification(string searchTerm)
        : this()
    {
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            Criteria = u =>
                (u.FirstName != null && u.FirstName.ToLower().Contains(term)) ||
                (u.LastName != null && u.LastName.ToLower().Contains(term)) ||
                (u.Email != null && u.Email.ToLower().Contains(term));
        }
    }
}
