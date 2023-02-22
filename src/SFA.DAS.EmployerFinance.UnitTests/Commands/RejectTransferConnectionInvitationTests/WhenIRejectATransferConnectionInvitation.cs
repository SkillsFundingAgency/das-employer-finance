﻿using SFA.DAS.EmployerFinance.Commands.RejectTransferConnectionInvitation;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Messages.Events;
using SFA.DAS.EmployerFinance.Models.Account;
using SFA.DAS.EmployerFinance.Models.TransferConnections;
using SFA.DAS.EmployerFinance.Models.UserProfile;
using SFA.DAS.EmployerFinance.TestCommon.Builders;
using SFA.DAS.UnitOfWork.Context;

namespace SFA.DAS.EmployerFinance.UnitTests.Commands.RejectTransferConnectionInvitation
{
    [TestFixture]
    public class WhenIRejectATransferConnectionInvitation
    {
        private RejectTransferConnectionInvitationCommandHandler _handler;
        private RejectTransferConnectionInvitationCommand _command;
        private Mock<IEmployerAccountRepository> _employerAccountRepository;
        private Mock<ITransferConnectionInvitationRepository> _transferConnectionInvitationRepository;
        private Mock<IUserAccountRepository> _userRepository;
        private TransferConnectionInvitation _transferConnectionInvitation;
        private UnitOfWorkContext _unitOfWorkContext;
        private Account _senderAccount;
        private Account _receiverAccount;
        private User _receiverUser;

        [SetUp]
        public void Arrange()
        {
            _employerAccountRepository = new Mock<IEmployerAccountRepository>();
            _transferConnectionInvitationRepository = new Mock<ITransferConnectionInvitationRepository>();
            _userRepository = new Mock<IUserAccountRepository>(); 
            
            var hashedAccountId = "ABC123";
            var publicHashedAccountId = "ABCDEFGHJKLMN12345";

            _receiverUser = new User
            {
                Id = 123456,
                Ref = Guid.NewGuid(),
                FirstName = "John",
                LastName = "Doe"
            };

            _senderAccount = new Account
            {
                Id = 333333,
                Name = "Sender",
                HashedId = hashedAccountId,
                PublicHashedId = publicHashedAccountId
            };

            _receiverAccount = new Account
            {
                Id = 222222,
                Name = "Receiver",
                HashedId = "DEF123",
                PublicHashedId = "GHHD3876"
            };

            _unitOfWorkContext = new UnitOfWorkContext();

            _transferConnectionInvitation = new TransferConnectionInvitationBuilder()
                .WithId(111111)
                .WithSenderAccount(_senderAccount)
                .WithReceiverAccount(_receiverAccount)
                .WithStatus(TransferConnectionInvitationStatus.Pending)
                .Build();
            
            _userRepository.Setup(r => r.Get(_receiverUser.Ref)).ReturnsAsync(_receiverUser);
            _employerAccountRepository.Setup(r => r.Get(_senderAccount.Id)).ReturnsAsync(_senderAccount);
            _employerAccountRepository.Setup(r => r.Get(_receiverAccount.Id)).ReturnsAsync(_receiverAccount);
            _transferConnectionInvitationRepository.Setup(r => r.Get(_transferConnectionInvitation.Id)).ReturnsAsync(_transferConnectionInvitation);

            _handler = new RejectTransferConnectionInvitationCommandHandler(
                _employerAccountRepository.Object,
                _transferConnectionInvitationRepository.Object,
                _userRepository.Object
            );

            _command = new RejectTransferConnectionInvitationCommand
            {
                AccountId = _receiverAccount.Id,
                UserRef = _receiverUser.Ref,
                TransferConnectionInvitationId = _transferConnectionInvitation.Id
            };
        }

        [Test]
        public async Task ThenShouldRejectTransferConnectionInvitation()
        {
            var now = DateTime.UtcNow;

            await _handler.Handle(_command, CancellationToken.None);

            Assert.That(_transferConnectionInvitation.Status, Is.EqualTo(TransferConnectionInvitationStatus.Rejected));
            Assert.That(_transferConnectionInvitation.Changes.Count, Is.EqualTo(1));

            var change = _transferConnectionInvitation.Changes.Single();

            Assert.That(change.CreatedDate, Is.GreaterThanOrEqualTo(now));
            Assert.That(change.DeletedByReceiver, Is.Null);
            Assert.That(change.DeletedBySender, Is.Null);
            Assert.That(change.ReceiverAccount, Is.Null);
            Assert.That(change.SenderAccount, Is.Null);
            Assert.That(change.Status, Is.EqualTo(TransferConnectionInvitationStatus.Rejected));
            Assert.That(change.User, Is.Not.Null);
            Assert.That(change.User.Id, Is.EqualTo(_receiverUser.Id));
        }

        [Test]
        public async Task ThenShouldPublishRejectedTransferConnectionInvitationEvent()
        {
            await _handler.Handle(_command, CancellationToken.None);

            var messages = _unitOfWorkContext.GetEvents().ToList();
            var message = messages.OfType<RejectedTransferConnectionRequestEvent>().FirstOrDefault();

            Assert.That(messages.Count, Is.EqualTo(1));
            Assert.That(message, Is.Not.Null);
            Assert.That(message.Created, Is.EqualTo(_transferConnectionInvitation.Changes.Select(c => c.CreatedDate).Cast<DateTime?>().SingleOrDefault()));
            Assert.That(message.ReceiverAccountHashedId, Is.EqualTo(_receiverAccount.HashedId));
            Assert.That(message.ReceiverAccountId, Is.EqualTo(_receiverAccount.Id));
            Assert.That(message.ReceiverAccountName, Is.EqualTo(_receiverAccount.Name));
            Assert.That(message.RejectorUserRef, Is.EqualTo(_receiverUser.Ref));
            Assert.That(message.RejectorUserId, Is.EqualTo(_receiverUser.Id));
            Assert.That(message.RejectorUserName, Is.EqualTo(_receiverUser.FullName));
            Assert.That(message.SenderAccountHashedId, Is.EqualTo(_senderAccount.HashedId));
            Assert.That(message.SenderAccountId, Is.EqualTo(_senderAccount.Id));
            Assert.That(message.SenderAccountName, Is.EqualTo(_senderAccount.Name));
            Assert.That(message.TransferConnectionRequestId, Is.EqualTo(_transferConnectionInvitation.Id));
        }

        [Test]
        public void ThenShouldThrowExceptionIfRejectorIsNotTheReceiver()
        {
            _command.AccountId = _senderAccount.Id;

            Assert.ThrowsAsync<Exception>(() => _handler.Handle(_command, CancellationToken.None), "Requires rejector account is the receiver account.");
        }

        [Test]
        public void ThenShouldThrowExceptionIfTransferConnectionInvitationIsNotPending()
        {
            _transferConnectionInvitation = new TransferConnectionInvitationBuilder()
                .WithId(111111)
                .WithSenderAccount(_senderAccount)
                .WithReceiverAccount(_receiverAccount)
                .WithStatus(TransferConnectionInvitationStatus.Rejected)
                .Build();

            _transferConnectionInvitationRepository.Setup(r => r.Get(_transferConnectionInvitation.Id)).ReturnsAsync(_transferConnectionInvitation);

            Assert.ThrowsAsync<Exception>(() => _handler.Handle(_command, CancellationToken.None), "Requires transfer connection invitation is pending.");
        }
    }
}