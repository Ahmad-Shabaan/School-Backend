using Application.Common;
using System.Net;

namespace Application.Features.Courses.Errors;

public static class CourseErrors
{
    public static readonly Error CourseNotFound = new("Course.NotFound", "Course was not found.", HttpStatusCode.NotFound);
    public static readonly Error UserNotFound = new("User.NotFound", "User was not found.", HttpStatusCode.NotFound);
    public static readonly Error InactiveUser = new("User.Inactive", "Your account has been deactivated.", HttpStatusCode.Forbidden);
    public static readonly Error FileRequired = new("Course.FileRequired", "Course file is required.", HttpStatusCode.BadRequest);
}