using Application.Features.Identity.DTOs;
using Application.Specifications;
using Domain.Entities;

namespace Application.Features.Identity.Specifications;

public class UserListItemProjectionSpecification
    : BaseProjectionSpecification<ApplicationUser, UserListItemDto>
{
    public UserListItemProjectionSpecification()
        : base(u => new UserListItemDto
        {
            Id = u.Id,
            FirstName = u.FirstName,
            LastName = u.LastName,
            Email = u.Email ?? string.Empty,
            UserName = u.UserName ?? string.Empty,
            IsActive = u.IsActive,
            CreatedAt = u.CreatedAt
        })
    {
        ApplyOrderByDescending(u => u.CreatedAt);
    }

    public UserListItemProjectionSpecification(string searchTerm)
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

    public UserListItemProjectionSpecification(string searchTerm, int page, int pageSize)
        : this(searchTerm)
    {
        ApplyPaging((page - 1) * pageSize, pageSize);
    }
}
