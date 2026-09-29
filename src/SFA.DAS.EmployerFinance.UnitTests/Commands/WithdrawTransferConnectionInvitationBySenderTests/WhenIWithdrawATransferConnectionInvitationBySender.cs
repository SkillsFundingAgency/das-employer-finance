using SFA.DAS.EmployerFinance.Commands.WithdrawTransferConnectionInvitationBySender;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Messages.Events;
using SFA.DAS.EmployerFinance.Models.Account;
using SFA.DAS.EmployerFinance.Models.TransferConnections;
using SFA.DAS.EmployerFinance.Models.UserProfile;
using SFA.DAS.UnitOfWork.Context;

namespace SFA.DAS.EmployerFinance.UnitTests.Commands.WithdrawTransferConnectionInvitationBySender;

public class WhenIWithdrawATransferConnectionInvitationBySender
{
    [Test]
    public async Task ThenShouldWithdrawTransferConnectionInvitation()
    {
        // Arrange
        var fixture = new WithdrawBySenderFixture().WithPendingInvitation();
        var now = DateTime.UtcNow;

        // Act
        await fixture.Handle();

        // Assert
        Assert.That(fixture.Invitation.Status, Is.EqualTo(TransferConnectionInvitationStatus.Rejected));
        Assert.That(fixture.Invitation.Changes.Count, Is.EqualTo(2));

        var change = fixture.Invitation.Changes.OrderByDescending(c => c.CreatedDate).First();

        Assert.That(change.CreatedDate, Is.GreaterThanOrEqualTo(now));
        Assert.That(change.Status, Is.EqualTo(TransferConnectionInvitationStatus.Rejected));
        Assert.That(change.User, Is.SameAs(fixture.SenderUser));

        fixture.VerifyRepositoriesCalled();
    }

    [Test]
    public async Task ThenShouldPublishRejectedTransferConnectionInvitationEventWithWithdrawnBySender()
    {
        // Arrange
        var fixture = new WithdrawBySenderFixture().WithPendingInvitation();

        // Act
        await fixture.Handle();

        // Assert
        var message = fixture.UnitOfWorkContext.GetEvents()
            .OfType<RejectedTransferConnectionRequestEvent>()
            .SingleOrDefault();

        Assert.That(message, Is.Not.Null);
        Assert.That(message.WithdrawnBySender, Is.True);
        Assert.That(message.ReceiverAccountId, Is.EqualTo(fixture.ReceiverAccount.Id));
        Assert.That(message.SenderAccountId, Is.EqualTo(fixture.SenderAccount.Id));
        Assert.That(message.RejectorUserName, Is.EqualTo("System"));
        Assert.That(message.RejectorUserId, Is.EqualTo(fixture.SenderUser.Id));

        fixture.VerifyRepositoriesCalled();
    }

    [Test]
    public void ThenShouldThrowExceptionIfPendingInvitationIsNotFound()
    {
        // Arrange
        var fixture = new WithdrawBySenderFixture().WithMissingInvitation();

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Handle());
    }

    [Test]
    public void ThenShouldThrowExceptionIfSenderAccountIsNotFound()
    {
        // Arrange
        var fixture = new WithdrawBySenderFixture().WithMissingSenderAccount();

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Handle());
    }

    [Test]
    public void ThenShouldThrowExceptionIfInvitationHasNoUserChange()
    {
        // Arrange
        var fixture = new WithdrawBySenderFixture().WithPendingInvitationWithoutUserChange();

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Handle());
    }

    private class WithdrawBySenderFixture
    {
        public Account SenderAccount { get; } = new()
        {
            Id = 333333,
            Name = "Sender",
            HashedId = "ABC123",
            PublicHashedId = "ABCDEFGHJKLMN12345"
        };

        public Account ReceiverAccount { get; } = new()
        {
            Id = 222222,
            Name = "Receiver",
            HashedId = "DEF123",
            PublicHashedId = "GHHD3876"
        };

        public User SenderUser { get; } = new()
        {
            Id = 42,
            Ref = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            FirstName = "Jane",
            LastName = "Doe",
            Email = "jane@example.com"
        };

        public TransferConnectionInvitation Invitation { get; private set; }
        public UnitOfWorkContext UnitOfWorkContext { get; } = new();

        private readonly Mock<IEmployerAccountRepository> _employerAccountRepository = new();
        private readonly Mock<ITransferConnectionInvitationRepository> _transferConnectionInvitationRepository = new();
        private WithdrawTransferConnectionInvitationBySenderCommand _command;

        public WithdrawBySenderFixture WithPendingInvitation()
        {
            Invitation = new TransferConnectionInvitation(SenderAccount, ReceiverAccount, SenderUser);
            SetInvitationId(Invitation, 111111);

            _employerAccountRepository.Setup(r => r.Get(SenderAccount.Id)).ReturnsAsync(SenderAccount);
            _transferConnectionInvitationRepository
                .Setup(r => r.GetBySender(Invitation.Id, SenderAccount.Id, TransferConnectionInvitationStatus.Pending))
                .ReturnsAsync(Invitation);

            _command = new WithdrawTransferConnectionInvitationBySenderCommand
            {
                SenderAccountId = SenderAccount.Id,
                TransferConnectionInvitationId = Invitation.Id
            };

            return this;
        }

        public WithdrawBySenderFixture WithPendingInvitationWithoutUserChange()
        {
            var invitation = new Mock<TransferConnectionInvitation> { CallBase = true };
            invitation.SetupProperty(i => i.Id, 111111);
            invitation.SetupProperty(i => i.SenderAccount, SenderAccount);
            invitation.SetupProperty(i => i.SenderAccountId, SenderAccount.Id);
            invitation.SetupProperty(i => i.ReceiverAccount, ReceiverAccount);
            invitation.SetupProperty(i => i.ReceiverAccountId, ReceiverAccount.Id);
            invitation.SetupProperty(i => i.Status, TransferConnectionInvitationStatus.Pending);
            Invitation = invitation.Object;

            _employerAccountRepository.Setup(r => r.Get(SenderAccount.Id)).ReturnsAsync(SenderAccount);
            _transferConnectionInvitationRepository
                .Setup(r => r.GetBySender(Invitation.Id, SenderAccount.Id, TransferConnectionInvitationStatus.Pending))
                .ReturnsAsync(Invitation);

            _command = new WithdrawTransferConnectionInvitationBySenderCommand
            {
                SenderAccountId = SenderAccount.Id,
                TransferConnectionInvitationId = Invitation.Id
            };

            return this;
        }

        public WithdrawBySenderFixture WithMissingInvitation()
        {
            _employerAccountRepository.Setup(r => r.Get(SenderAccount.Id)).ReturnsAsync(SenderAccount);
            _transferConnectionInvitationRepository
                .Setup(r => r.GetBySender(111111, SenderAccount.Id, TransferConnectionInvitationStatus.Pending))
                .ReturnsAsync((TransferConnectionInvitation)null);

            _command = new WithdrawTransferConnectionInvitationBySenderCommand
            {
                SenderAccountId = SenderAccount.Id,
                TransferConnectionInvitationId = 111111
            };

            return this;
        }

        public WithdrawBySenderFixture WithMissingSenderAccount()
        {
            _employerAccountRepository.Setup(r => r.Get(SenderAccount.Id)).ReturnsAsync((Account)null);
            _transferConnectionInvitationRepository
                .Setup(r => r.GetBySender(111111, SenderAccount.Id, TransferConnectionInvitationStatus.Pending))
                .ReturnsAsync((TransferConnectionInvitation)null);

            _command = new WithdrawTransferConnectionInvitationBySenderCommand
            {
                SenderAccountId = SenderAccount.Id,
                TransferConnectionInvitationId = 111111
            };

            return this;
        }

        public Task Handle()
        {
            var handler = new WithdrawTransferConnectionInvitationBySenderCommandHandler(
                _employerAccountRepository.Object,
                _transferConnectionInvitationRepository.Object);

            return handler.Handle(_command, CancellationToken.None);
        }

        public void VerifyRepositoriesCalled()
        {
            _employerAccountRepository.Verify(r => r.Get(SenderAccount.Id), Times.Once);
            _transferConnectionInvitationRepository.Verify(
                r => r.GetBySender(Invitation.Id, SenderAccount.Id, TransferConnectionInvitationStatus.Pending),
                Times.Once);
        }

        private static void SetInvitationId(TransferConnectionInvitation invitation, int id)
        {
            typeof(TransferConnectionInvitation)
                .GetProperty(nameof(TransferConnectionInvitation.Id))!
                .SetValue(invitation, id);
        }
    }
}
