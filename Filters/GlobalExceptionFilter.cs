using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Authentication.Filters
{
    public class GlobalExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            var statusCode = StatusCodes.Status500InternalServerError;
            var message = "An unexpected error occurred.";


            var response = new
            {
                error = message
            };

            context.Result = new JsonResult(response)
            {
                StatusCode = statusCode,
            };
        }
    }
}
