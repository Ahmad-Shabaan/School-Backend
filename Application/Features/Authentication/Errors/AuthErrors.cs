using Application.Common;
using System.Net;

namespace Application.Features.Authentication.Errors
{
    public static class AuthErrors
    {
        public static readonly Error InvalidRefreshToken =new( "Auth.InvalidRefreshToken","The refresh token is invalid.",HttpStatusCode.Unauthorized);
        public static readonly Error InvalidCredentials = new("Auth.InvalidCredentials", "Invalid email or password", HttpStatusCode.FailedDependency);
        public static readonly Error AccessDenied = new("Auth.AccessDenied", "You have not permission.", HttpStatusCode.Forbidden);
        public static readonly Error UserNotFound = new("User.NotFound", "User was not found.", HttpStatusCode.NotFound);

    }
}
