using Application.Specifications;
using Domain.Entities;

namespace Application.Features.Enrollments.Specifications;

public class UsersByEmailsSpecification
    : BaseSpecification<ApplicationUser>
{
    public UsersByEmailsSpecification(IEnumerable<string> emails)
    {
        var emailList = emails
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim().ToLower())
            .Distinct()
            .ToList();

        Criteria = x =>
            x.Email != null &&
            emailList.Contains(x.Email.ToLower());
    }
}