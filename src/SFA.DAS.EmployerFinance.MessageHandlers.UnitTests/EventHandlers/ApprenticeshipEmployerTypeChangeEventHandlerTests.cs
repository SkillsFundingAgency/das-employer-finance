using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EmployerAccounts.Messages.Events;
using SFA.DAS.EmployerFinance.Commands.WithdrawTransferConnectionInvitationBySender;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.MessageHandlers.EventHandlers;
using SFA.DAS.EmployerFinance.Models.Account;
using SFA.DAS.EmployerFinance.Models.TransferConnections;
using SFA.DAS.EmployerFinance.TestCommon.Builders;

namespace SFA.DAS.EmployerFinance.MessageHandlers.UnitTests.EventHandlers;

public class ApprenticeshipEmployerTypeChangeEventHandlerTests
{
    private const long SenderAccountId = 1001;
    private const int FirstInvitationId = 10;
    private const int SecondInvitationId = 11;

    [Test]
    public async Task Handle_WhenEmployerBecomesNonLevy_ThenWithdrawCommandsAreSentForPendingSenderInvitations()
    {
        // Arrange
        var repository = new Mock<ITransferConnectionInvitationRepository>();
        var mediator = new Mock<IMediator>();
        var pendingInvitations = new List<TransferConnectionInvitation>
        {
            CreatePendingInvitation(SenderAccountId, FirstInvitationId),
            CreatePendingInvitation(SenderAccountId, SecondInvitationId),
            CreatePendingInvitation(2002, 12)
        };

        repository.Setup(r => r.GetPendingBySender(SenderAccountId))
            .ReturnsAsync(pendingInvitations.Where(i => i.SenderAccountId == SenderAccountId).ToList());

        var handler = new ApprenticeshipEmployerTypeChangeEventHandler(
            repository.Object,
            mediator.Object,
            Mock.Of<ILogger<ApprenticeshipEmployerTypeChangeEventHandler>>());

        var @event = new ApprenticeshipEmployerTypeChangeEvent
        {
            AccountId = SenderAccountId,
            ApprenticeshipEmployerType = ApprenticeshipEmployerType.NonLevy,
            Created = DateTime.UtcNow
        };

        // Act
        await handler.Handle(@event, null);

        // Assert
        repository.Verify(r => r.GetPendingBySender(SenderAccountId), Times.Once);

        mediator.Verify(m => m.Send(
                It.Is<WithdrawTransferConnectionInvitationBySenderCommand>(c =>
                    c.SenderAccountId == SenderAccountId && c.TransferConnectionInvitationId == FirstInvitationId),
                CancellationToken.None),
            Times.Once);

        mediator.Verify(m => m.Send(
                It.Is<WithdrawTransferConnectionInvitationBySenderCommand>(c =>
                    c.SenderAccountId == SenderAccountId && c.TransferConnectionInvitationId == SecondInvitationId),
                CancellationToken.None),
            Times.Once);
    }

    [Test]
    public async Task Handle_WhenEmployerBecomesLevy_ThenNoWithdrawCommandsAreSent()
    {
        // Arrange
        var repository = new Mock<ITransferConnectionInvitationRepository>();
        var mediator = new Mock<IMediator>();

        var handler = new ApprenticeshipEmployerTypeChangeEventHandler(
            repository.Object,
            mediator.Object,
            Mock.Of<ILogger<ApprenticeshipEmployerTypeChangeEventHandler>>());

        var @event = new ApprenticeshipEmployerTypeChangeEvent
        {
            AccountId = SenderAccountId,
            ApprenticeshipEmployerType = ApprenticeshipEmployerType.Levy,
            Created = DateTime.UtcNow
        };

        // Act
        await handler.Handle(@event, null);

        // Assert
        repository.Verify(r => r.GetPendingBySender(SenderAccountId), Times.Never);

        mediator.Verify(m => m.Send(
                It.Is<WithdrawTransferConnectionInvitationBySenderCommand>(c =>
                    c.SenderAccountId == SenderAccountId && c.TransferConnectionInvitationId == FirstInvitationId),
                CancellationToken.None),
            Times.Never);
    }

    private static TransferConnectionInvitation CreatePendingInvitation(long senderAccountId, int invitationId)
    {
        var senderAccount = new Account { Id = senderAccountId, Name = $"Sender {senderAccountId}" };
        var receiverAccount = new Account { Id = senderAccountId + 1, Name = $"Receiver {invitationId}" };

        return new TransferConnectionInvitationBuilder()
            .WithId(invitationId)
            .WithSenderAccount(senderAccount)
            .WithReceiverAccount(receiverAccount)
            .WithStatus(TransferConnectionInvitationStatus.Pending)
            .Build();
    }
}
