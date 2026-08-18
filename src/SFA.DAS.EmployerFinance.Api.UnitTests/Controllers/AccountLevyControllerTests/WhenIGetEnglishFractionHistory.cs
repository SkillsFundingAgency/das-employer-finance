using System.ComponentModel.DataAnnotations;
using SFA.DAS.EmployerFinance.Queries.GetEnglishFractionHistory;

namespace SFA.DAS.EmployerFinance.Api.UnitTests.Controllers.AccountLevyControllerTests;

[TestFixture]
public class WhenIGetEnglishFractionHistory : AccountLevyControllerTests
{
    [Test]
    public async Task ThenValidationExceptionIsThrownWhenEmpRefIsInvalid()
    {
        var hashedAccountId = "ABC123";
        var invalidEmpRef = "778<>/GDS00004";
        var validationResult = new System.ComponentModel.DataAnnotations.ValidationResult(
            "Validation failed",
            ["EmpRef|EmpRef must be a valid PAYE reference"]);

        Mediator
            .Setup(x => x.Send(
                It.Is<GetEnglishFractionHistoryQuery>(q =>
                    q.HashedAccountId == hashedAccountId && q.EmpRef == invalidEmpRef),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException(validationResult, null, null));

        var act = () => Controller.GetEnglishFractionHistory(hashedAccountId, invalidEmpRef);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
