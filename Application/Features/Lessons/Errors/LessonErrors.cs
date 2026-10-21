using Application.Common;
using System.Net;

namespace Application.Features.Lessons.Errors;

public static class LessonErrors
{
    public static readonly Error LessonNotFound = new("Lesson.NotFound", "Lesson was not found.", HttpStatusCode.NotFound);
    public static readonly Error CourseNotFound = new("Course.NotFound", "Course was not found.", HttpStatusCode.NotFound);
    public static readonly Error NotAuthorized = new("Lesson.NotAuthorized", "You are not authorized to manage lessons for this course.", HttpStatusCode.Forbidden);
    public static readonly Error InvalidYouTubeUrl = new("Lesson.InvalidYouTubeUrl", "The provided YouTube URL or video ID is invalid.", HttpStatusCode.BadRequest);
    public static readonly Error InvalidMeetingUrl = new("Lesson.InvalidMeetingUrl", "The provided meeting URL is invalid.", HttpStatusCode.BadRequest);
    public static readonly Error LessonLocked = new("Lesson.Locked", "The lesson is not unlocked yet.", HttpStatusCode.Forbidden);
    public static readonly Error NotEnrolled = new("Lesson.NotEnrolled", "You are not enrolled in this course.", HttpStatusCode.Forbidden);
    public static readonly Error DuplicateOrder = new("Lesson.DuplicateOrder", "Another lesson in this course already uses this order.", HttpStatusCode.BadRequest);
    public static readonly Error InvalidReorder = new("Lesson.InvalidReorder", "The reorder payload does not match the lessons of this course.", HttpStatusCode.BadRequest);
}
