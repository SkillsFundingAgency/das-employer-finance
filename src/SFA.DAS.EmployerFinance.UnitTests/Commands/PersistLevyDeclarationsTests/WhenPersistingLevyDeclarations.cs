using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerFinance.Api.Types;
using SFA.DAS.EmployerFinance.Commands.PersistLevyDeclarations;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Models.Levy;
using SFA.DAS.EmployerFinance.Validation;
using ValidationResult = SFA.DAS.EmployerFinance.Validation.ValidationResult;

namespace SFA.DAS.EmployerFinance.UnitTests.Commands.PersistLevyDeclarationsTests;

public class WhenPersistingLevyDeclarations
{
    private Mock<IValidator<PersistLevyDeclarationsCommand>> _validator = null!;
    private Mock<IDasLevyRepository> _repository = null!;
    private Mock<ILogger<PersistLevyDeclarationsCommandHandler>> _logger = null!;
    private PersistLevyDeclarationsCommandHandler _handler = null!;

    [SetUp]
    public void Arrange()
    {
        _validator = new Mock<IValidator<PersistLevyDeclarationsCommand>>();
        _validator
            .Setup(x => x.Validate(It.IsAny<PersistLevyDeclarationsCommand>()))
            .Returns(new ValidationResult());

        _repository = new Mock<IDasLevyRepository>();
        _logger = new Mock<ILogger<PersistLevyDeclarationsCommandHandler>>();
        _handler = new PersistLevyDeclarationsCommandHandler(
            _validator.Object,
            _repository.Object,
            _logger.Object);
    }

    [Test]
    public void Then_Does_Not_Persist_When_Validation_Fails()
    {
        var command = CreateCommand();
        var validationResult = new ValidationResult();
        validationResult.AddError(nameof(command.Data.AccountId), "AccountId is invalid.");
        _validator.Setup(x => x.Validate(command)).Returns(validationResult);

        Assert.ThrowsAsync<ValidationException>(
            () => _handler.Handle(command, CancellationToken.None));

        _repository.Verify(
            x => x.PersistLevyDeclarations(
                It.IsAny<IEnumerable<DasDeclaration>>(),
                It.IsAny<string>(),
                It.IsAny<long>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task Then_Persists_Normalized_Declarations_And_Returns_Exact_Metrics()
    {
        var command = CreateCommand();
        _repository
            .Setup(x => x.PersistLevyDeclarations(
                It.Is<IEnumerable<DasDeclaration>>(declarations => DeclarationsMatch(declarations)),
                command.Data.EmpRef,
                command.Data.AccountId,
                true,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LevyPersistenceResult
            {
                DeclarationsPersisted = 2,
                LevyTransactionValue = 175.25m,
                TransactionsCreated = 2
            });

        var result = await _handler.Handle(command, CancellationToken.None);

        result.DeclarationsReceived.Should().Be(2);
        result.DeclarationsPersisted.Should().Be(2);
        result.DeclarationsSkipped.Should().Be(0);
        result.LevyTransactionValue.Should().Be(175.25m);
        result.TransactionsCreated.Should().Be(2);
        _repository.Verify(
            x => x.PersistLevyDeclarations(
                It.Is<IEnumerable<DasDeclaration>>(declarations => DeclarationsMatch(declarations)),
                command.Data.EmpRef,
                command.Data.AccountId,
                true,
                It.IsAny<CancellationToken>()),
            Times.Once);
        _logger.VerifyLogging(
            "[CorrelationId: corr-123] Persist levy declarations completed for AccountId 123, EmpRef 123/ABC, persisted 2, skipped 0, levy transaction total 175.25, transactions created 2",
            LogLevel.Information,
            Times.Once());
    }

    [Test]
    public async Task Then_Replaying_The_Same_Payload_Returns_All_Declarations_As_Skipped()
    {
        var command = CreateCommand();
        var invocation = 0;
        _repository
            .Setup(x => x.PersistLevyDeclarations(
                It.IsAny<IEnumerable<DasDeclaration>>(),
                command.Data.EmpRef,
                command.Data.AccountId,
                true,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => ++invocation == 1
                ? new LevyPersistenceResult { DeclarationsPersisted = 2, TransactionsCreated = 2 }
                : new LevyPersistenceResult { DeclarationsPersisted = 0, TransactionsCreated = 0 });

        var first = await _handler.Handle(command, CancellationToken.None);
        var replay = await _handler.Handle(command, CancellationToken.None);

        first.DeclarationsReceived.Should().Be(2);
        first.TransactionsCreated.Should().Be(2);
        replay.DeclarationsReceived.Should().Be(2);
        replay.DeclarationsPersisted.Should().Be(0);
        replay.DeclarationsSkipped.Should().Be(2);
        replay.TransactionsCreated.Should().Be(0);
        _repository.Verify(
            x => x.PersistLevyDeclarations(
                It.IsAny<IEnumerable<DasDeclaration>>(),
                command.Data.EmpRef,
                command.Data.AccountId,
                true,
                It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    [Test]
    public async Task Then_Does_Not_Request_Transaction_Creation_When_Disabled()
    {
        var command = CreateCommand();
        command.Data.GenerateTransactions = false;
        _repository
            .Setup(x => x.PersistLevyDeclarations(
                It.IsAny<IEnumerable<DasDeclaration>>(),
                command.Data.EmpRef,
                command.Data.AccountId,
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LevyPersistenceResult { DeclarationsPersisted = 2 });

        var result = await _handler.Handle(command, CancellationToken.None);

        result.DeclarationsReceived.Should().Be(2);
        result.DeclarationsPersisted.Should().Be(2);
        result.DeclarationsSkipped.Should().Be(0);
        result.LevyTransactionValue.Should().Be(0);
        result.TransactionsCreated.Should().Be(0);
        _repository.VerifyAll();
    }

    [Test]
    public async Task Then_Logs_Context_And_Rethrows_When_Persistence_Fails()
    {
        var command = CreateCommand();
        var expectedException = new InvalidOperationException("Database unavailable");
        _repository
            .Setup(x => x.PersistLevyDeclarations(
                It.IsAny<IEnumerable<DasDeclaration>>(),
                command.Data.EmpRef,
                command.Data.AccountId,
                true,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        var actualException = Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));

        actualException.Should().BeSameAs(expectedException);
        _logger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, _) => state.ToString() ==
                    "[CorrelationId: corr-123] Persist levy declarations failed for AccountId 123, EmpRef 123/ABC, declarations received 2"),
                It.Is<Exception>(exception => ReferenceEquals(exception, expectedException)),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    private static bool DeclarationsMatch(IEnumerable<DasDeclaration> declarations)
    {
        var actual = declarations.ToList();
        return actual.Count == 2
               && actual[0].Id == "1001"
               && actual[0].SubmissionType == "FPS"
               && !actual[0].EndOfYearAdjustment
               && actual[1].Id == "1002"
               && actual[1].SubmissionType == "EPS"
               && actual[1].EndOfYearAdjustment
               && actual[1].EndOfYearAdjustmentAmount == 50m;
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
                GenerateTransactions = true,
                Declarations =
                [
                    new NormalizedLevyDeclaration
                    {
                        Id = "1001",
                        SubmissionId = 2001,
                        SubmissionDate = new DateTime(2026, 4, 1),
                        SubmissionType = "FPS",
                        PayrollYear = "26-27",
                        PayrollMonth = 1,
                        LevyDueYtd = 100,
                        LevyAllowanceForFullYear = 15000
                    },
                    new NormalizedLevyDeclaration
                    {
                        Id = "1002",
                        SubmissionId = 2002,
                        SubmissionDate = new DateTime(2026, 5, 1),
                        SubmissionType = "EPS",
                        PayrollYear = "26-27",
                        PayrollMonth = 2,
                        LevyDueYtd = 150,
                        LevyAllowanceForFullYear = 15000,
                        EndOfYearAdjustment = true,
                        EndOfYearAdjustmentAmount = 50
                    }
                ]
            }
        };
    }
}
