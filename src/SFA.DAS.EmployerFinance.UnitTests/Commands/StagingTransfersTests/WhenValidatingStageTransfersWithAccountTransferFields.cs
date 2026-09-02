using SFA.DAS.EmployerFinance.Commands.StagingTransfers;
using SFA.DAS.EmployerFinance.Models.Transfers;
using SFA.DAS.EmployerFinance.Validation;

namespace SFA.DAS.EmployerFinance.UnitTests.Commands.StagingTransfersTests;

public class WhenValidatingStageTransfersWithAccountTransferFields
{
    private StageTransfersCommandValidator _validator;

    [SetUp]
    public void Arrange()
    {
        _validator = new StageTransfersCommandValidator();
    }

    [Test]
    public void Then_Is_Valid_When_AccountTransfer_Fields_Are_Present()
    {
        var result = _validator.Validate(new StageTransfersCommand
        {
            Transfers =
            [
                new TransferStaging
                {
                    TransferId = 1,
                    SenderAccountId = 10,
                    ReceiverAccountId = 20,
                    Amount = 100,
                    PeriodEnd = "2526-R01",
                    ApprenticeshipId = 99,
                    Type = "Levy",
                    RequiredPaymentId = Guid.NewGuid()
                }
            ]
        });

        result.IsValid().Should().BeTrue();
    }

    [Test]
    public void Then_Is_Valid_When_RequiredPaymentId_Is_Empty()
    {
        var result = _validator.Validate(new StageTransfersCommand
        {
            Transfers = [CreateTransfer(requiredPaymentId: Guid.Empty)]
        });

        result.IsValid().Should().BeTrue();
    }

    [Test]
    public void Then_Is_Valid_When_Amount_Is_Zero_Or_Negative()
    {
        var result = _validator.Validate(new StageTransfersCommand
        {
            Transfers =
            [
                CreateTransfer(transferId: 1, amount: 0m),
                CreateTransfer(transferId: 2, amount: -50m)
            ]
        });

        result.IsValid().Should().BeTrue();
    }

    [Test]
    public void Then_Is_Valid_When_ApprenticeshipId_Is_Zero()
    {
        var result = _validator.Validate(new StageTransfersCommand
        {
            Transfers = [CreateTransfer(apprenticeshipId: 0)]
        });

        result.IsValid().Should().BeTrue();
    }

    [Test]
    public void Then_Uses_Indexed_Keys_When_Two_Transfers_Have_Invalid_TransferIds()
    {
        ValidationResult result = null;

        var act = () =>
        {
            result = _validator.Validate(new StageTransfersCommand
            {
                Transfers =
                [
                    CreateTransfer(transferId: 0),
                    CreateTransfer(transferId: 0)
                ]
            });
        };

        act.Should().NotThrow();
        result.IsValid().Should().BeFalse();
        result.ValidationDictionary.Keys.Should().BeEquivalentTo("Transfers[0].TransferId", "Transfers[1].TransferId");
    }

    [Test]
    public void Then_Does_Not_Throw_When_Two_Transfers_Would_Have_Shared_The_Amount_Key()
    {
        var result = _validator.Validate(new StageTransfersCommand
        {
            Transfers =
            [
                CreateTransfer(transferId: 1, amount: 0m),
                CreateTransfer(transferId: 2, amount: -10m)
            ]
        });

        result.IsValid().Should().BeTrue();
    }

    private static TransferStaging CreateTransfer(
        long transferId = 1,
        decimal amount = 100,
        long apprenticeshipId = 99,
        Guid? requiredPaymentId = null)
    {
        return new TransferStaging
        {
            TransferId = transferId,
            SenderAccountId = 10,
            ReceiverAccountId = 20,
            Amount = amount,
            PeriodEnd = "2526-R01",
            ApprenticeshipId = apprenticeshipId,
            Type = "Levy",
            RequiredPaymentId = requiredPaymentId ?? Guid.NewGuid()
        };
    }
}
