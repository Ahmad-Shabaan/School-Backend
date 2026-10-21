using Application.Common;
using System.Net;

namespace Application.Features.Quizzes.Errors;

public static class QuizErrors
{
    public static readonly Error QuizNotFound = new("Quiz.NotFound", "Quiz was not found.", HttpStatusCode.NotFound);
    public static readonly Error IncompleteAnswers = new("Quiz.IncompleteAnswers", "All quiz questions must be answered exactly once.", HttpStatusCode.BadRequest);
    public static readonly Error InvalidAnswers = new("Quiz.InvalidAnswers", "One or more submitted answers are invalid for this quiz.", HttpStatusCode.BadRequest);
    public static readonly Error DuplicateAnswers = new("Quiz.DuplicateAnswers", "A question cannot be answered more than once.", HttpStatusCode.BadRequest);
    public static readonly Error QuizAlreadyExists = new("Quiz.AlreadyExists", "A quiz already exists for this lesson.", HttpStatusCode.BadRequest);
    public static readonly Error HasAttempts = new("Quiz.HasAttempts", "The quiz cannot be deleted, and its questions cannot be changed, after students have submitted attempts.", HttpStatusCode.BadRequest);
}
