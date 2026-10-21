using Application.Common;
using Application.Features.Quizzes.DTOs;
using MediatR;

namespace Application.Features.Quizzes.Commands.CreateQuiz;

public class CreateQuizCommand : IRequest<Result<QuizManagementDto>>, IQuizPayload
{
    public int LessonId { get; set; }

    public string Title { get; set; } = string.Empty;

    public decimal PassingScorePercent { get; set; } = 80m;

    public List<QuizQuestionInput> Questions { get; set; } = new();
}
