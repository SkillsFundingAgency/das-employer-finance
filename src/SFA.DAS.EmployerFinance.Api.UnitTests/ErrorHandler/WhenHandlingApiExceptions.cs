using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using SFA.DAS.EmployerFinance.Api.ErrorHandler;

namespace SFA.DAS.EmployerFinance.Api.UnitTests.ErrorHandler;

[TestFixture]
public class WhenHandlingApiExceptions
{
    [Test]
    public void ThenGetValidationErrorsMapsMemberNamesToDictionary()
    {
        var validationResult = new ValidationResult(
            "Validation failed",
            ["EmpRef|EmpRef must be a valid PAYE reference"]);
        var exception = new ValidationException(validationResult, null, null);

        var errors = ErrorHandlerExtensions.GetValidationErrors(exception);

        errors.Should().ContainKey("EmpRef");
        errors["EmpRef"].Should().Be("EmpRef must be a valid PAYE reference");
    }

    [Test]
    public void ThenGetValidationErrorsFallsBackToMessageWhenValidationResultIsNull()
    {
        var exception = new ValidationException("Something was invalid");

        var errors = ErrorHandlerExtensions.GetValidationErrors(exception);

        errors.Should().ContainKey("Validation");
        errors["Validation"].Should().Be("Something was invalid");
    }

    [Test]
    public async Task ThenValidationExceptionReturnsBadRequestWithErrorDictionary()
    {
        var validationResult = new ValidationResult(
            "Validation failed",
            ["HashedAccountId|HashedAccountId has not been supplied"]);
        var exception = new ValidationException(validationResult, null, null);
        var context = CreateExceptionContext(exception);

        await ErrorHandlerExtensions.HandleExceptionAsync(context, NullLogger.Instance);

        context.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        context.Response.ContentType.Should().Be("application/json");

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body, System.Text.Encoding.UTF8);
        var body = await reader.ReadToEndAsync();
        var errors = JsonSerializer.Deserialize<Dictionary<string, string>>(body);

        errors.Should().NotBeNull();
        errors!.Should().ContainKey("HashedAccountId");
        errors["HashedAccountId"].Should().Be("HashedAccountId has not been supplied");
    }

    [Test]
    public async Task ThenOtherExceptionsReturnInternalServerError()
    {
        var context = CreateExceptionContext(new InvalidOperationException("boom"));

        await ErrorHandlerExtensions.HandleExceptionAsync(context, NullLogger.Instance);

        context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }

    private static DefaultHttpContext CreateExceptionContext(Exception exception)
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.Features.Set<IExceptionHandlerFeature>(new ExceptionHandlerFeature { Error = exception });
        return context;
    }
}
