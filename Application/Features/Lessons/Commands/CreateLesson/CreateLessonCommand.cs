using Application.Common;
using Application.Features.Lessons.DTOs;
using MediatR;

namespace Application.Features.Lessons.Commands.CreateLesson;

public class CreateLessonCommand : IRequest<Result<LessonDetailsDto>>
{
    public int CourseId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string? YouTubeUrl { get; set; }

    public string? MeetingUrl { get; set; }

    public DateTime? MeetingDateTime { get; set; }

    public List<string> Goals { get; set; } = new();

    public List<LessonResourceInput> Resources { get; set; } = new();
}
