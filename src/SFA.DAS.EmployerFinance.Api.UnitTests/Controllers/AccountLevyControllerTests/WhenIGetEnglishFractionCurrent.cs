using System.ComponentModel.DataAnnotations;
using SFA.DAS.EmployerFinance.Queries.GetEnglishFractionCurrent;

namespace SFA.DAS.EmployerFinance.Api.UnitTests.Controllers.AccountLevyControllerTests;

[TestFixture]
public class WhenIGetEnglishFractionCurrent : AccountLevyControllerTests
{
    [Test]
    public async Task ThenValidationExceptionIsThrownWhenEmpRefIsInvalid()
    {
        var hashedAccountId = "ABC123";
        var invalidEmpRefs = new[] { "778<>/GDS00004" };
        var validationResult = new System.ComponentModel.DataAnnotations.ValidationResult(
            "Validation failed",
            ["EmpRefs|EmpRefs[0] must be a valid PAYE reference"]);

        Mediator
            .Setup(x => x.Send(
                It.Is<GetEnglishFractionCurrentQuery>(q =>
                    q.HashedAccountId == hashedAccountId &&
                    q.EmpRefs.SequenceEqual(invalidEmpRefs)),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException(validationResult, null, null));

        var act = () => Controller.GetEnglishFractionCurrent(invalidEmpRefs, hashedAccountId);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
