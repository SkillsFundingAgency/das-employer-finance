using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace SFA.DAS.EmployerFinance.Api.ErrorHandler;

public static class ErrorHandlerExtensions
{
    public static IApplicationBuilder UseApiGlobalExceptionHandler(this IApplicationBuilder app, ILogger logger)
    {
        app.UseExceptionHandler(appError =>
        {
            appError.Run(context => HandleExceptionAsync(context, logger));
        });
        return app;
    }

    public static async Task HandleExceptionAsync(HttpContext context, ILogger logger)
    {
        context.Response.ContentType = "application/json";

        var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
        if (contextFeature?.Error == null)
        {
            return;
        }

        if (contextFeature.Error is ValidationException validationException)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            var errors = GetValidationErrors(validationException);
            await context.Response.WriteAsync(JsonSerializer.Serialize(errors));
            return;
        }

        logger.LogError(contextFeature.Error, "Something went wrong: {Error}", contextFeature.Error);
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
    }

    public static Dictionary<string, string> GetValidationErrors(ValidationException ex)
    {
        var errors = ex.ValidationResult?.MemberNames
            .Select(x => x.Split('|', 2))
            .Where(x => x.Length == 2)
            .ToDictionary(x => x[0], x => x[1]);

        if (errors == null || errors.Count == 0)
        {
            return new Dictionary<string, string> { { "Validation", ex.Message } };
        }

        return errors;
    }
}
