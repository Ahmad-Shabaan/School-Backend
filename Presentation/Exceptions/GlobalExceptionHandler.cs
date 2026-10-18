using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Exceptions
{
    public class GlobalExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
    {
        async ValueTask<bool> IExceptionHandler.TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            ProblemDetails problme = exception switch
            {
                ValidationException ex => new ValidationProblemDetails(
                    ex.Errors.GroupBy(g => g.PropertyName).ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())
                    )
                {
                    Title = "Validtion Error",
                    Detail = "",
                    Status = StatusCodes.Status400BadRequest

                },
                _ => new ProblemDetails()
                {
                    Title = "Server Error",
                    Detail = "An unhandled Exception",
                    Status = StatusCodes.Status500InternalServerError
                }

            };
            httpContext.Response.StatusCode = problme.Status!.Value;
            await problemDetailsService.WriteAsync(new ProblemDetailsContext()
            {
                HttpContext = httpContext,
                ProblemDetails = problme
            });
            return true;
        }
    }
}
