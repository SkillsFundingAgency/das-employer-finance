using SFA.DAS.EmployerFinance.Commands.DeleteTransferConnectionInvitation;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Messages.Events;
using SFA.DAS.EmployerFinance.Models.Account;
using SFA.DAS.EmployerFinance.Models.TransferConnections;
using SFA.DAS.EmployerFinance.Models.UserProfile;
using SFA.DAS.EmployerFinance.TestCommon.Builders;
using SFA.DAS.Testing;
using SFA.DAS.UnitOfWork.Context;

namespace SFA.DAS.EmployerFinance.UnitTests.Commands
{
    [TestFixture]
    public class DeleteTransferConnectionInvitationTests : FluentTest<DeleteTransferConnectionInvitationTestFixture>
    {
        [TestCase(DeleteTransferConnectionInvitationTestFixture.Constants.TestSenderAccountId)]
        [TestCase(DeleteTransferConnectionInvitationTestFixture.Constants.TestReceiverAccountId)]
        public Task Handle_WhenMakingAValidCall_ThenShouldVerifyDeletingAccountExists(long deletingAccountId)
        {
            return TestAsync(act: f => f.Handle(TransferConnectionInvitationStatus.Rejected, deletingAccountId), 
                assert: f => f.EmployerAccountRepositoryMock.Verify(r => r.Get(deletingAccountId), Times.Once));
        }

        [TestCase(DeleteTransferConnectionInvitationTestFixture.Constants.TestSenderAccountId)]
        [TestCase(DeleteTransferConnectionInvitationTestFixture.Constants.TestReceiverAccountId)]
        public Task Handle_WhenMakingAValidCall_ThenShouldVerifyUserExists(long deletingAccountId)
        {
            return TestAsync(act: f => f.Handle(TransferConnectionInvitationStatus.Rejected, deletingAccountId),
                assert: f => f.UserRepositoryMock.Verify(r => r.Get(f.DeleterUser.Ref), Times.Once));
        }

        [TestCase(DeleteTransferConnectionInvitationTestFixture.Constants.TestSenderAccountId)]
        [TestCase(DeleteTransferConnectionInvitationTestFixture.Constants.TestReceiverAccountId)]
        public Task Handle_WhenMakingAValidCall_ThenShouldVerifyTransferConnectionInvitationExists(long deletingAccountId)
        {
            return TestAsync(act: f => f.Handle(TransferConnectionInvitationStatus.Rejected, deletingAccountId),
                assert: f => f.TransferConnectionInvitationRepositoryMock.Verify(r => r.Get(f.TransferConnectionInvitation.Id), Times.Once));
        }

        [TestCase(DeleteTransferConnectionInvitationTestFixture.Constants.TestSenderAccountId)]
        [TestCase(DeleteTransferConnectionInvitationTestFixture.Constants.TestReceiverAccountId)]
        public Task Handle_WhenMakingAValidCall_ThenInvitationShouldEndInRejectedStatus(long deletingAccountId)
        {
            return TestAsync(act: f => f.Handle(TransferConnectionInvitationStatus.Rejected, deletingAccountId),
                assert: f => Assert.That(f.TransferConnectionInvitation.Status, Is.EqualTo(TransferConnectionInvitationStatus.Rejected)));
        }

        [Test]
        public Task Handle_WhenSenderDeleting_ThenShouldBeDeletedBySender()
        {
            return TestAsync(act: f => f.Handle(TransferConnectionInvitationStatus.Rejected, DeleteTransferConnectionInvitationTestFixture.Constants.TestSenderAccountId),
                assert: f =>Assert.That(f.TransferConnectionInvitation.DeletedBySender, Is.True));
        }

        [Test]
        public Task Handle_WhenreceiverDeleting_ThenShouldBeDeletedByReceiver()
        {
            return TestAsync(act: f => f.Handle(TransferConnectionInvitationStatus.Rejected, DeleteTransferConnectionInvitationTestFixture.Constants.TestReceiverAccountId),
                assert: f => Assert.That(f.TransferConnectionInvitation.DeletedByReceiver, Is.True));
        }

        [TestCase(DeleteTransferConnectionInvitationTestFixture.Constants.TestSenderAccountId)]
        [TestCase(DeleteTransferConnectionInvitationTestFixture.Constants.TestReceiverAccountId)]
        public Task Handle_WhenDeleting_ThenShouldBeOneChangeEntry(long deletingAccountId)
        {
            return TestAsync(act: f => f.Handle(TransferConnectionInvitationStatus.Rejected, deletingAccountId),
                assert: f => Assert.That(f.TransferConnectionInvitation.Changes.Count, Is.EqualTo(1)));
        }

        [TestCase(DeleteTransferConnectionInvitationTestFixture.Constants.TestSenderAccountId)]
        [TestCase(DeleteTransferConnectionInvitationTestFixture.Constants.TestReceiverAccountId)]
        public Task Handle_WhenSenderDeleting_ThenChangeEntryShouldBeCorrect(long deletingAccountId)
        {
            var now = DateTime.UtcNow;

            return TestAsync(act: f => f.Handle(TransferConnectionInvitationStatus.Rejected, deletingAccountId),
                assert: f =>
                {
                    var change = f.TransferConnectionInvitation.Changes.Single();

                    Assert.That(change.CreatedDate, Is.GreaterThanOrEqualTo(now));
                    Assert.That(change.Status, Is.Null);
                    Assert.That(change.User, Is.Not.Null);
                    Assert.That(change.User.Id, Is.EqualTo(f.DeleterUser.Id));
                });
        }

        [TestCase(DeleteTransferConnectionInvitationTestFixture.Constants.TestSenderAccountId)]
        [TestCase(DeleteTransferConnectionInvitationTestFixture.Constants.TestReceiverAccountId)]
        public Task Handle_WhenDeleting_ThenSingleEventShouldBeCreated(long deletingAccountId)
        {
            return TestAsync(act: f => f.Handle(TransferConnectionInvitationStatus.Rejected, deletingAccountId),
                assert: f => Assert.That(f.UnitOfWorkContext.GetEvents().OfType<DeletedTransferConnectionRequestEvent>().Count(), Is.EqualTo(1)));
        }

        [TestCase(DeleteTransferConnectionInvitationTestFixture.Constants.TestSenderAccountId)]
        [TestCase(DeleteTransferConnectionInvitationTestFixture.Constants.TestReceiverAccountId)]
        public Task Handle_WhenDeleting_ThenSingleEventShouldBeSetCorrectly(long deletingAccountId)
        {
            return TestAsync(act: f => f.Handle(TransferConnectionInvitationStatus.Rejected, deletingAccountId),
                assert: f =>
                {
                    var message = f.UnitOfWorkContext.GetEvents().OfType<DeletedTransferConnectionRequestEvent>().Single();
                    Assert.That(message, Is.Not.Null);
                    Assert.That(message.DeletedByAccountId, Is.EqualTo(deletingAccountId));
                    Assert.That(message.DeletedByUserRef, Is.EqualTo(f.DeleterUser.Ref));
                    Assert.That(message.DeletedByUserId, Is.EqualTo(f.DeleterUser.Id));
                    Assert.That(message.DeletedByUserName, Is.EqualTo(f.DeleterUser.FullName));
                    Assert.That(message.Created, Is.EqualTo(f.TransferConnectionInvitation.Changes.Select(c => c.CreatedDate).Cast<DateTime?>().SingleOrDefault()));
                    Assert.That(message.ReceiverAccountHashedId, Is.EqualTo(f.ReceiverAccount.HashedId));
                    Assert.That(message.ReceiverAccountId, Is.EqualTo(f.ReceiverAccount.Id));
                    Assert.That(message.ReceiverAccountName, Is.EqualTo(f.ReceiverAccount.Name));
                    Assert.That(message.SenderAccountHashedId, Is.EqualTo(f.SenderAccount.HashedId));
                    Assert.That(message.SenderAccountId, Is.EqualTo(f.SenderAccount.Id));
                    Assert.That(message.SenderAccountName, Is.EqualTo(f.SenderAccount.Name));
                    Assert.That(message.TransferConnectionRequestId, Is.EqualTo(f.TransferConnectionInvitation.Id));
                });
        }

        public Task Handle_WhenSenderDeleting_ThenShouldLookLikeDeletedBySender()
        {
            return TestAsync(act: f => f.Handle(TransferConnectionInvitationStatus.Rejected, DeleteTransferConnectionInvitationTestFixture.Constants.TestSenderAccountId),
                assert: f =>
                {
                    f.TransferConnectionInvitation.DeletedBySender.Should().BeTrue();
                    f.TransferConnectionInvitation.DeletedByReceiver.Should().BeFalse();
                    (f.TransferConnectionInvitation.Changes.SingleOrDefault(tcic => 
                                      tcic.DeletedBySender.HasValue && 
                                      tcic.DeletedBySender.Value && !tcic.DeletedByReceiver.HasValue) != null).Should().BeTrue();
                });
        }

        public Task Handle_WhenReceiverDeleting_ThenShouldLookLikeDeletedByReceiver()
        {
            return TestAsync(act: f => f.Handle(TransferConnectionInvitationStatus.Rejected, DeleteTransferConnectionInvitationTestFixture.Constants.TestReceiverAccountId),
                assert: f =>
                {
                    f.TransferConnectionInvitation.DeletedByReceiver.Should().BeTrue();
                    f.TransferConnectionInvitation.DeletedBySender.Should().BeFalse();
                    (f.TransferConnectionInvitation.Changes.SingleOrDefault(tcic =>
                                      tcic.DeletedByReceiver.HasValue && 
                                      tcic.DeletedByReceiver.Value &&
                                      !tcic.DeletedBySender.HasValue) != null).Should().BeTrue();
                });
        }

        [TestCase(DeleteTransferConnectionInvitationTestFixture.Constants.TestSenderAccountId, TransferConnectionInvitationStatus.Approved)]
        [TestCase(DeleteTransferConnectionInvitationTestFixture.Constants.TestReceiverAccountId, TransferConnectionInvitationStatus.Approved)]
        [TestCase(DeleteTransferConnectionInvitationTestFixture.Constants.TestSenderAccountId, TransferConnectionInvitationStatus.Pending)]
        [TestCase(DeleteTransferConnectionInvitationTestFixture.Constants.TestReceiverAccountId, TransferConnectionInvitationStatus.Pending)]
        public void Handle_WhenDeleting_ThenShouldThrowExceptionIfNotRejected(long deletingAccountId, TransferConnectionInvitationStatus status)
        {
            Assert.ThrowsAsync<InvalidOperationException>(() =>
                TestAsync(
                    act: f => f.Handle(status, deletingAccountId),
                    assert: null), "Requires transfer connection invitation is rejected.");
        }
    }

    public class DeleteTransferConnectionInvitationTestFixture 
    {
        public DeleteTransferConnectionInvitationTestFixture()
        {
            EmployerAccountRepositoryMock = new Mock<IEmployerAccountRepository>();
            TransferConnectionInvitationRepositoryMock = new Mock<ITransferConnectionInvitationRepository>();
            UserRepositoryMock = new Mock<IUserAccountRepository>();
            UnitOfWorkContext = new UnitOfWorkContext();
        }

        public Mock<IEmployerAccountRepository> EmployerAccountRepositoryMock;
        public IEmployerAccountRepository EmployerAccountRepository => EmployerAccountRepositoryMock.Object;

        public Mock<ITransferConnectionInvitationRepository> TransferConnectionInvitationRepositoryMock;
        public ITransferConnectionInvitationRepository TransferConnectionInvitationRepository => TransferConnectionInvitationRepositoryMock.Object;

        public Mock<IUserAccountRepository> UserRepositoryMock;
        public IUserAccountRepository UserRepository => UserRepositoryMock.Object;

        public Account SenderAccount { get; private set; }
        public Account ReceiverAccount { get; private set; }
        public User DeleterUser { get; private set; }
        public TransferConnectionInvitation TransferConnectionInvitation { get; private set; }
        public IUnitOfWorkContext UnitOfWorkContext { get; }

        public DeleteTransferConnectionInvitationTestFixture WithSenderAccount(long senderAccountId)
        {
            var hashedAccountId = "ABC123";

            SenderAccount = new Account
            {
                Id = senderAccountId,
                Name = "Sender",
                HashedId = hashedAccountId
            };

            EmployerAccountRepositoryMock
                .Setup(r => r.Get(SenderAccount.Id))
                .ReturnsAsync(SenderAccount);

            return this;
        }

        public DeleteTransferConnectionInvitationTestFixture WithReceiverAccount(long receiverAccountId)
        {
            var hashedAccountId = "ABC123";
            ReceiverAccount = new Account
            {
                Id = receiverAccountId,
                Name = "Receiver",
                HashedId = hashedAccountId
            };

            EmployerAccountRepositoryMock
                .Setup(r => r.Get(ReceiverAccount.Id))
                .ReturnsAsync(ReceiverAccount);

            return this;
        }

        public DeleteTransferConnectionInvitationTestFixture WithDeleterUser(long deletedUserId)
        {
            DeleterUser = new User
            {
                Id = deletedUserId,
                Ref = Guid.NewGuid(),
                FirstName = "John",
                LastName = "Doe"
            };

            UserRepositoryMock
                .Setup(r => r.Get(DeleterUser.Ref))
                .ReturnsAsync(DeleterUser);

            UserRepositoryMock
                .Setup(r => r.Get(DeleterUser.Ref))
                .ReturnsAsync(DeleterUser);

            return this;
        }

        public DeleteTransferConnectionInvitationTestFixture WithTransferConnection(
            TransferConnectionInvitationStatus status)
        {
            TransferConnectionInvitation = new TransferConnectionInvitationBuilder()
                .WithId(111111)
                .WithSenderAccount(SenderAccount)
                .WithReceiverAccount(ReceiverAccount)
                .WithStatus(status)
                .Build();

            TransferConnectionInvitationRepositoryMock
                .Setup(r => r.Get(TransferConnectionInvitation.Id))
                .ReturnsAsync(TransferConnectionInvitation);

            return this;
        }

        public static class Constants
        {
            public const long TestSenderAccountId = 123;
            public const long TestReceiverAccountId = 456;
            public const long TestUserId = 789;
        }

        public static long TestSenderAccountId => Constants.TestSenderAccountId;

        public static long TestReceiverAccountId => Constants.TestReceiverAccountId;

        public static long TestUserId => Constants.TestUserId;

        public Task Handle(TransferConnectionInvitationStatus status, long deletingAccountId)
        {
            WithSenderAccount(TestSenderAccountId)
                .WithReceiverAccount(TestReceiverAccountId)
                .WithDeleterUser(TestUserId)
                .WithTransferConnection(status);

            var command = new DeleteTransferConnectionInvitationCommand
            {
                AccountId = deletingAccountId,
                UserRef = DeleterUser.Ref,
                TransferConnectionInvitationId = TransferConnectionInvitation.Id
            };

            var handler = CreateHandler();

            return handler.Handle(command, CancellationToken.None);
        }

        private DeleteTransferConnectionInvitationCommandHandler CreateHandler()
        {
            return new DeleteTransferConnectionInvitationCommandHandler(
                EmployerAccountRepository,
                TransferConnectionInvitationRepository,
                UserRepository
            );
        }
    }
}