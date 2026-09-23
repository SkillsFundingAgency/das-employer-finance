using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture.NUnit4;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Jobs.ScheduledJobs;
using SFA.DAS.EmployerFinance.Messages.Commands;
using SFA.DAS.EmployerFinance.Models.Payments;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.EmployerFinance.Jobs.UnitTests.ScheduledJobs;

public class RepairMissingPaymentsMetadataTests
{
    [Test, MoqAutoData]
    public async Task WhenPaymentsWithMissingMetadataIsNullThenNoMessagesAreSent(
        Mock<ILogger> logger,
        [Frozen] Mock<IMessageSession> messageSession,
        [Frozen] Mock<IDasLevyRepository> levyRepository,
        RepairMissingPaymentsMetadata sut)
    {
        levyRepository
            .Setup(x => x.GetPaymentsWithMissingMetadata(RepairMissingPaymentsMetadata.PageSize, RepairMissingPaymentsMetadata.PageNumber))
            .ReturnsAsync(() => null)
            .Verifiable();

        await sut.Run(null, logger.Object);

        levyRepository.Verify();
        
        messageSession.Verify(x => x.Send(It.IsAny<ImportAccountPaymentMetadataCommand>(), It.IsAny<SendOptions>()), Times.Never);
    }
    
    [Test, MoqAutoData]
    public async Task WhenThereAreNoPaymentsWithMissingMetadataThenNoMessagesAreSent(
        Mock<ILogger> logger,
        [Frozen] Mock<IMessageSession> messageSession,
        [Frozen] Mock<IDasLevyRepository> levyRepository,
        RepairMissingPaymentsMetadata sut)
    {
        levyRepository
            .Setup(x => x.GetPaymentsWithMissingMetadata(RepairMissingPaymentsMetadata.PageSize, RepairMissingPaymentsMetadata.PageNumber))
            .ReturnsAsync(() => [])
            .Verifiable();

        await sut.Run(null, logger.Object);

        levyRepository.Verify();
        
        messageSession.Verify(x => x.Send(It.IsAny<ImportAccountPaymentMetadataCommand>(), It.IsAny<SendOptions>()), Times.Never);
    }

    [Test, MoqAutoData]
    public async Task WhenThereArePaymentsWithMissingMetadataThenMessagesAreSentForEachPayment(
        Mock<ILogger> logger,
        List<PaymentDetails> paymentDetailsList,
        [Frozen] Mock<IMessageSession> messageSession,
        [Frozen] Mock<IDasLevyRepository> levyRepository,
        RepairMissingPaymentsMetadata sut)
    {
        levyRepository
            .Setup(x => x.GetPaymentsWithMissingMetadata(RepairMissingPaymentsMetadata.PageSize, RepairMissingPaymentsMetadata.PageNumber))
            .ReturnsAsync(() => paymentDetailsList)
            .Verifiable();

        await sut.Run(null, logger.Object);

        levyRepository.Verify();

        foreach (var paymentDetails in paymentDetailsList)
        {
            messageSession.Verify(x => x.Send(It.Is<ImportAccountPaymentMetadataCommand>(
                    c => c.AccountId == paymentDetails.EmployerAccountId
                         && c.PeriodEndRef == paymentDetails.PeriodEnd
                         && c.PaymentId == paymentDetails.Id
                ), It.IsAny<SendOptions>()),
                Times.Once);
        }
    }

    [Test]
    public void PageSizeIsDrivenFromCode()
    {
        RepairMissingPaymentsMetadata.PageSize.Should().Be(500);
        RepairMissingPaymentsMetadata.PageNumber.Should().Be(1);
    }
}
