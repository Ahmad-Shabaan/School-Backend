using Application.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Identity.Errors
{
    public static class UserErrors
    {
        public static readonly Error UserNotFound = new Error("User.NotFound", "User was not found.", HttpStatusCode.NotFound);
        public static readonly Error AlreadyExists = new("User.AlreadyExists", "User already exists", HttpStatusCode.Conflict);
        public static readonly Error RoleNotFount = new("Role.NotFound", "Role you have tried to assign does not exist.", HttpStatusCode.NotFound);
        public static readonly Error RoleAlreadyExists = new("Role.AlreadyExists", "Role already exists", HttpStatusCode.Conflict);

    }
}
