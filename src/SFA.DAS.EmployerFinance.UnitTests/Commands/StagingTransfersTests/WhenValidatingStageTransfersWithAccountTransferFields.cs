using SFA.DAS.EmployerFinance.Commands.StagingTransfers;
using SFA.DAS.EmployerFinance.Models.Transfers;

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
    public void Then_Is_Invalid_When_RequiredPaymentId_Is_Empty()
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
                    RequiredPaymentId = Guid.Empty
                }
            ]
        });

        result.IsValid().Should().BeFalse();
        result.ValidationDictionary.Values.Should().Contain("RequiredPaymentId is required");
    }
}
