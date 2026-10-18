using Application.Common;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Presentation.Exceptions
{

    //public sealed record StatusCodeResponse(HttpStatusCode StatusCode, ProblemDetails Problem);

    public static class GlobalErrorsHandler
    {

        public static ProblemDetails Handle(Result result)
        {
            ProblemDetails problem = new()
            {
                Title = result.Message,
                Status = (int)HttpStatusCode.BadRequest

            };

            if (result.Error is not null)
            {
                problem.Title = result.Error.Code;
                problem.Detail = result.Error.Message;
                problem.Status = (int)result.Error.StatusCode;
            }
            ;

            if (result.Errors is not null)
            {
                var gErrors = result.Errors.GroupBy(e => e.Code).ToDictionary(g => g.Key, g => g.Select(e => e.Message).ToArray());

                foreach (var item in gErrors)
                {
                    problem.Extensions.Add(item.Key, item.Value);
                }
            }

            return problem;

        }
    }

}
