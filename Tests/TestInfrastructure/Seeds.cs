using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Tests.TestInfrastructure;

public static class Seeds
{
    public static Task<ApplicationUser> CreateUserAsync(
        this TestHost host,
        string? email = null,
        string firstName = "Test",
        string lastName = "Student")
    {
        email ??= $"student-{Guid.NewGuid():N}@school.test";

        return host.ExecuteAsync(async context =>
        {
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                NormalizedEmail = email.ToUpperInvariant(),
                UserName = email,
                NormalizedUserName = email.ToUpperInvariant(),
                EmailConfirmed = true,
                IsActive = true,
                ForceChangedPassword = false,
                PasswordHash = "placeholder"
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            return user;
        });
    }

    public static Task<Course> CreateCourseAsync(
        this TestHost host,
        string name = "Test Course")
    {
        return host.ExecuteAsync(async context =>
        {
            var course = new Course
            {
                CourseName = name,
                Description = "Course used in automated tests."
            };

            context.Courses.Add(course);
            await context.SaveChangesAsync();

            return course;
        });
    }

    public static Task<Lesson> CreateLessonAsync(
        this TestHost host,
        int courseId,
        int order,
        string title = "Lesson",
        string? videoId = "dQw4w9WgXcQ")
    {
        return host.ExecuteAsync(async context =>
        {
            var lesson = new Lesson
            {
                CourseId = courseId,
                Order = order,
                Title = $"{title} {order}",
                Description = "Lesson description.",
                YouTubeVideoId = videoId
            };

            context.Lessons.Add(lesson);
            await context.SaveChangesAsync();

            return lesson;
        });
    }

    public static Task EnrollAsync(
        this TestHost host,
        Guid applicationUserId,
        int courseId)
    {
        return host.ExecuteAsync(async context =>
        {
            context.UserCourses.Add(new UserCourse
            {
                ApplicationUserId = applicationUserId,
                CourseId = courseId
            });

            await context.SaveChangesAsync();
        });
    }

    public static Task<Quiz> CreateQuizAsync(
        this TestHost host,
        int lessonId,
        int questionCount = 4,
        decimal passingScorePercent = 80m)
    {
        return host.ExecuteAsync(async context =>
        {
            var quiz = new Quiz
            {
                LessonId = lessonId,
                Title = "Test Quiz",
                PassingScorePercent = passingScorePercent
            };

            for (var i = 0; i < questionCount; i++)
            {
                var question = new Question
                {
                    Text = $"Question {i + 1}"
                };

                question.Options.Add(new AnswerOption
                {
                    Text = $"Correct answer {i + 1}",
                    IsCorrect = true
                });

                question.Options.Add(new AnswerOption
                {
                    Text = $"Wrong answer {i + 1}",
                    IsCorrect = false
                });

                quiz.Questions.Add(question);
            }

            context.Quizzes.Add(quiz);
            await context.SaveChangesAsync();

            return quiz;
        });
    }

    public static Task<LessonComment> CreateCommentAsync(
        this TestHost host,
        int lessonId,
        Guid authorId,
        string content = "A test comment",
        int? parentCommentId = null)
    {
        return host.ExecuteAsync(async context =>
        {
            var comment = new LessonComment
            {
                LessonId = lessonId,
                AuthorId = authorId,
                Content = content,
                ParentCommentId = parentCommentId
            };

            context.LessonComments.Add(comment);
            await context.SaveChangesAsync();

            return comment;
        });
    }

    public static Task<List<AnswerOption>> GetQuizOptionsAsync(
        this TestHost host,
        int quizId)
    {
        return host.ExecuteAsync(context =>
            context.AnswerOptions
                .Where(o => o.Question.QuizId == quizId)
                .OrderBy(o => o.QuestionId)
                .ThenBy(o => o.Id)
                .ToListAsync());
    }

    public static Task<Quiz> GetQuizByLessonAsync(
        this TestHost host,
        int lessonId)
    {
        return host.ExecuteAsync(context =>
            context.Quizzes
                .Include(q => q.Questions)
                .ThenInclude(q => q.Options)
                .FirstAsync(q => q.LessonId == lessonId));
    }
}
