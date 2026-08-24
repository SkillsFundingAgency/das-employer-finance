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
    public void Then_Request_Data_Is_Required()
    {
        var command = new PersistLevyDeclarationsCommand { Data = null! };

        var result = _validator.Validate(command);

        result.ValidationDictionary.Should().Contain(
            nameof(command.Data),
            "Request payload is required.");
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void Then_AccountId_Must_Be_Greater_Than_Zero(long accountId)
    {
        var command = CreateCommand();
        command.Data.AccountId = accountId;

        var result = _validator.Validate(command);

        result.ValidationDictionary.Should().Contain(
            nameof(command.Data.AccountId),
            "AccountId must be supplied and greater than zero.");
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
    [TestCase("   ")]
    public void Then_EmpRef_Is_Required(string empRef)
    {
        var command = CreateCommand();
        command.Data.EmpRef = empRef;

        var result = _validator.Validate(command);

        result.ValidationDictionary.Should().Contain(
            nameof(command.Data.EmpRef),
            "EmpRef has not been supplied.");
    }

    [Test]
    public void Then_A_NonEmpty_Declarations_Collection_Is_Required()
    {
        var command = CreateCommand();
        command.Data.Declarations = [];

        var result = _validator.Validate(command);

        result.ValidationDictionary.Should().Contain(
            nameof(command.Data.Declarations),
            "A non-empty Declarations collection is required.");
    }

    [Test]
    public void Then_A_Null_Declarations_Collection_Is_Rejected()
    {
        var command = CreateCommand();
        command.Data.Declarations = null!;

        var result = _validator.Validate(command);

        result.ValidationDictionary.Should().Contain(
            nameof(command.Data.Declarations),
            "A non-empty Declarations collection is required.");
    }

    [Test]
    public void Then_A_Null_Declaration_Entry_Is_Rejected()
    {
        var command = CreateCommand();
        command.Data.Declarations[0] = null!;

        var result = _validator.Validate(command);

        result.ValidationDictionary.Should().Contain(
            "Declarations[0]",
            "Declaration entry is required.");
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

    [Test]
    public void Then_Declaration_SubmissionId_Must_Be_Greater_Than_Zero()
    {
        var command = CreateCommand();
        command.Data.Declarations[0].SubmissionId = 0;

        var result = _validator.Validate(command);

        result.ValidationDictionary.Values.Should().Contain("SubmissionId must be greater than zero.");
    }

    [Test]
    public void Then_Declaration_PayrollYear_Is_Required()
    {
        var command = CreateCommand();
        command.Data.Declarations[0].PayrollYear = string.Empty;

        var result = _validator.Validate(command);

        result.ValidationDictionary.Values.Should().Contain("PayrollYear has not been supplied.");
    }

    [TestCase(null, "PayrollMonth has not been supplied.")]
    [TestCase((short)0, "PayrollMonth must be between 1 and 12.")]
    [TestCase((short)13, "PayrollMonth must be between 1 and 12.")]
    public void Then_Declaration_PayrollMonth_Must_Be_In_Range(short? payrollMonth, string expectedError)
    {
        var command = CreateCommand();
        command.Data.Declarations[0].PayrollMonth = payrollMonth;

        var result = _validator.Validate(command);

        result.ValidationDictionary.Values.Should().Contain(expectedError);
    }

    [Test]
    public void Then_Declaration_SubmissionDate_Is_Required()
    {
        var command = CreateCommand();
        command.Data.Declarations[0].SubmissionDate = DateTime.MinValue;

        var result = _validator.Validate(command);

        result.ValidationDictionary.Values.Should().Contain("SubmissionDate has not been supplied.");
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
