using SFA.DAS.EmployerFinance.Api.Types;
using SFA.DAS.EmployerFinance.Commands.PersistLevyDeclarations;
using SFA.DAS.EmployerFinance.Validation;

namespace SFA.DAS.EmployerFinance.UnitTests.Commands.PersistLevyDeclarationsTests;

public class WhenValidatingPersistLevyDeclarations
{
    private PersistLevyDeclarationsCommandValidator _validator = null!;

    [SetUp]
    public void Arrange()
    {
        _validator = new PersistLevyDeclarationsCommandValidator();
    }

    [Test]
    public void Then_A_Complete_Request_Is_Valid()
    {
        var result = _validator.Validate(CreateCommand());

        result.IsValid().Should().BeTrue();
    }

    [Test]
    public void Then_CorrelationId_Is_Required()
    {
        var command = CreateCommand();
        command.Data.CorrelationId = string.Empty;

        var result = _validator.Validate(command);

        result.ValidationDictionary.Values.Should().Contain("CorrelationId has not been supplied.");
    }

    [TestCase("")]
    [TestCase("not-a-number")]
    [TestCase("0")]
    [TestCase("-1")]
    public void Then_Declaration_Id_Must_Be_A_Positive_Whole_Number(string declarationId)
    {
        var command = CreateCommand();
        command.Data.Declarations[0].Id = declarationId;

        var result = _validator.Validate(command);

        result.ValidationDictionary.Values.Should().Contain("Id must be a positive whole number.");
    }

    private static PersistLevyDeclarationsCommand CreateCommand()
    {
        return new PersistLevyDeclarationsCommand
        {
            Data = new PersistLevyDeclarationRequestData
            {
                CorrelationId = "corr-123",
                AccountId = 123,
                EmpRef = "123/ABC",
                Declarations =
                [
                    new NormalizedLevyDeclaration
                    {
                        Id = "1001",
                        SubmissionId = 2001,
                        SubmissionDate = new DateTime(2026, 4, 1),
                        PayrollYear = "26-27",
                        PayrollMonth = 1
                    }
                ]
            }
        };
    }
}
