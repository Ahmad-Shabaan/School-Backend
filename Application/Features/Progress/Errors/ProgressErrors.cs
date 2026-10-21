using Application.Common;
using System.Net;

namespace Application.Features.Progress.Errors;

public static class ProgressErrors
{
    public static readonly Error InvalidDuration = new("Progress.InvalidDuration", "The reported video duration is invalid.", HttpStatusCode.BadRequest);
    public static readonly Error InvalidPosition = new("Progress.InvalidPosition", "The reported playback position is invalid.", HttpStatusCode.BadRequest);
    public static readonly Error InvalidSegment = new("Progress.InvalidSegment", "The reported watch segment is invalid.", HttpStatusCode.BadRequest);
    public static readonly Error SegmentTooLong = new("Progress.SegmentTooLong", "The reported watch segment is too long; report progress periodically instead.", HttpStatusCode.BadRequest);
    public static readonly Error LessonHasNoVideo = new("Progress.LessonHasNoVideo", "The lesson has no video to track.", HttpStatusCode.BadRequest);
}
