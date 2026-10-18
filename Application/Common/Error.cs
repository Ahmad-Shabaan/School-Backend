using Microsoft.AspNetCore.Identity;
using System.Net;
namespace Application.Common
{
    public sealed record Error(
        string Code,
        string Message,
        HttpStatusCode StatusCode);

}

