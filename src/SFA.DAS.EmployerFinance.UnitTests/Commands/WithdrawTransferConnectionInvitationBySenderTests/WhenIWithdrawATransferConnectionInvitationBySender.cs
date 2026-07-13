using SFA.DAS.EmployerFinance.Commands.WithdrawTransferConnectionInvitationBySender;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Messages.Events;
using SFA.DAS.EmployerFinance.Models.Account;
using SFA.DAS.EmployerFinance.Models.TransferConnections;
using SFA.DAS.EmployerFinance.TestCommon.Builders;
using SFA.DAS.UnitOfWork.Context;

namespace SFA.DAS.EmployerFinance.UnitTests.Commands.WithdrawTransferConnectionInvitationBySender;

public class WhenIWithdrawATransferConnectionInvitationBySender
{
    [Test]
    public async Task ThenShouldWithdrawTransferConnectionInvitation()
    {
        // Arrange
        var employerAccountRepository = new Mock<IEmployerAccountRepository>();
        var transferConnectionInvitationRepository = new Mock<ITransferConnectionInvitationRepository>();
        var senderAccount = new Account
        {
            Id = 333333,
            Name = "Sender",
            HashedId = "ABC123",
            PublicHashedId = "ABCDEFGHJKLMN12345"
        };
        var receiverAccount = new Account
        {
            Id = 222222,
            Name = "Receiver",
            HashedId = "DEF123",
            PublicHashedId = "GHHD3876"
        };
        var transferConnectionInvitation = new TransferConnectionInvitationBuilder()
            .WithId(111111)
            .WithSenderAccount(senderAccount)
            .WithReceiverAccount(receiverAccount)
            .WithStatus(TransferConnectionInvitationStatus.Pending)
            .Build();

        employerAccountRepository.Setup(r => r.Get(senderAccount.Id)).ReturnsAsync(senderAccount);
        transferConnectionInvitationRepository
            .Setup(r => r.GetBySender(transferConnectionInvitation.Id, senderAccount.Id, TransferConnectionInvitationStatus.Pending))
            .ReturnsAsync(transferConnectionInvitation);

        var handler = new WithdrawTransferConnectionInvitationBySenderCommandHandler(
            employerAccountRepository.Object,
            transferConnectionInvitationRepository.Object);

        var command = new WithdrawTransferConnectionInvitationBySenderCommand
        {
            SenderAccountId = senderAccount.Id,
            TransferConnectionInvitationId = transferConnectionInvitation.Id
        };

        _ = new UnitOfWorkContext();
        var now = DateTime.UtcNow;

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(transferConnectionInvitation.Status, Is.EqualTo(TransferConnectionInvitationStatus.Rejected));
        Assert.That(transferConnectionInvitation.Changes.Count, Is.EqualTo(1));

        var change = transferConnectionInvitation.Changes.Single();

        Assert.That(change.CreatedDate, Is.GreaterThanOrEqualTo(now));
        Assert.That(change.Status, Is.EqualTo(TransferConnectionInvitationStatus.Rejected));
        Assert.That(change.User, Is.Not.Null);
        Assert.That(change.User.FirstName, Is.EqualTo("System"));

        employerAccountRepository.Verify(r => r.Get(senderAccount.Id), Times.Once);
        transferConnectionInvitationRepository.Verify(
            r => r.GetBySender(transferConnectionInvitation.Id, senderAccount.Id, TransferConnectionInvitationStatus.Pending),
            Times.Once);
    }

    [Test]
    public async Task ThenShouldPublishRejectedTransferConnectionInvitationEventWithWithdrawnBySender()
    {
        // Arrange
        var employerAccountRepository = new Mock<IEmployerAccountRepository>();
        var transferConnectionInvitationRepository = new Mock<ITransferConnectionInvitationRepository>();
        var unitOfWorkContext = new UnitOfWorkContext();
        var senderAccount = new Account
        {
            Id = 333333,
            Name = "Sender",
            HashedId = "ABC123",
            PublicHashedId = "ABCDEFGHJKLMN12345"
        };
        var receiverAccount = new Account
        {
            Id = 222222,
            Name = "Receiver",
            HashedId = "DEF123",
            PublicHashedId = "GHHD3876"
        };
        var transferConnectionInvitation = new TransferConnectionInvitationBuilder()
            .WithId(111111)
            .WithSenderAccount(senderAccount)
            .WithReceiverAccount(receiverAccount)
            .WithStatus(TransferConnectionInvitationStatus.Pending)
            .Build();

        employerAccountRepository.Setup(r => r.Get(senderAccount.Id)).ReturnsAsync(senderAccount);
        transferConnectionInvitationRepository
            .Setup(r => r.GetBySender(transferConnectionInvitation.Id, senderAccount.Id, TransferConnectionInvitationStatus.Pending))
            .ReturnsAsync(transferConnectionInvitation);

        var handler = new WithdrawTransferConnectionInvitationBySenderCommandHandler(
            employerAccountRepository.Object,
            transferConnectionInvitationRepository.Object);

        var command = new WithdrawTransferConnectionInvitationBySenderCommand
        {
            SenderAccountId = senderAccount.Id,
            TransferConnectionInvitationId = transferConnectionInvitation.Id
        };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var messages = unitOfWorkContext.GetEvents().ToList();
        var message = messages.OfType<RejectedTransferConnectionRequestEvent>().FirstOrDefault();

        Assert.That(messages.Count, Is.EqualTo(1));
        Assert.That(message, Is.Not.Null);
        Assert.That(message.WithdrawnBySender, Is.True);
        Assert.That(message.ReceiverAccountId, Is.EqualTo(receiverAccount.Id));
        Assert.That(message.SenderAccountId, Is.EqualTo(senderAccount.Id));
        Assert.That(message.RejectorUserName, Is.EqualTo("System"));

        employerAccountRepository.Verify(r => r.Get(senderAccount.Id), Times.Once);
        transferConnectionInvitationRepository.Verify(
            r => r.GetBySender(transferConnectionInvitation.Id, senderAccount.Id, TransferConnectionInvitationStatus.Pending),
            Times.Once);
    }

    [Test]
    public void ThenShouldThrowExceptionIfSenderAccountDoesNotMatch()
    {
        // Arrange
        const long wrongSenderAccountId = 999999;
        const int invitationId = 111111;

        var employerAccountRepository = new Mock<IEmployerAccountRepository>();
        var transferConnectionInvitationRepository = new Mock<ITransferConnectionInvitationRepository>();

        employerAccountRepository.Setup(r => r.Get(wrongSenderAccountId)).ReturnsAsync((Account)null);
        transferConnectionInvitationRepository
            .Setup(r => r.GetBySender(invitationId, wrongSenderAccountId, TransferConnectionInvitationStatus.Pending))
            .ReturnsAsync((TransferConnectionInvitation)null);

        var handler = new WithdrawTransferConnectionInvitationBySenderCommandHandler(
            employerAccountRepository.Object,
            transferConnectionInvitationRepository.Object);

        var command = new WithdrawTransferConnectionInvitationBySenderCommand
        {
            SenderAccountId = wrongSenderAccountId,
            TransferConnectionInvitationId = invitationId
        };

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, CancellationToken.None));

        employerAccountRepository.Verify(r => r.Get(wrongSenderAccountId), Times.Once);
        transferConnectionInvitationRepository.Verify(
            r => r.GetBySender(invitationId, wrongSenderAccountId, TransferConnectionInvitationStatus.Pending),
            Times.Once);
    }
}
