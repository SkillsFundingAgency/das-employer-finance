using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Mappings;
using SFA.DAS.EmployerFinance.Models.Account;
using SFA.DAS.EmployerFinance.Models.TransferConnections;
using SFA.DAS.EmployerFinance.Queries.SendTransferConnectionInvitation;
using SFA.DAS.EmployerFinance.TestCommon.Helpers;

namespace SFA.DAS.EmployerFinance.UnitTests.Queries.SendTransferConnectionInvitationTests
{
    [TestFixture]
    public class WhenISendTransferConnectionInvitation
    {
        private SendTransferConnectionInvitationQueryHandler _handler;
        private SendTransferConnectionInvitationQuery _query;
        private SendTransferConnectionInvitationResponse _response;

        private Mock<IEmployerAccountRepository> _employerAccountRepository;
        private Mock<ITransferConnectionInvitationRepository> _transferConnectionInvitationRepository;
        private IMapper _mapper;

        private Account _receiverAccount;
        private Account _senderAccount;

        [SetUp]
        public void Arrange()
        {
            var hashedAccountId = "ABC123";
            _receiverAccount = new Account
            {
                Id = 111111,
                HashedId = hashedAccountId,
                PublicHashedId = "XYZ987"
                //PublicHashingService = new TestPublicHashingService()
            };

            _senderAccount = new Account
            {
                Id = 222222,
                HashedId = hashedAccountId,
                PublicHashedId="DEF678"
                //PublicHashingService = new TestPublicHashingService()
            };

            _employerAccountRepository = new Mock<IEmployerAccountRepository>();
            _employerAccountRepository.Setup(s => s.Get("111111")).ReturnsAsync(_receiverAccount);
            _employerAccountRepository.Setup(s => s.Get(_senderAccount.Id)).ReturnsAsync(_senderAccount);

            _transferConnectionInvitationRepository = new Mock<ITransferConnectionInvitationRepository>();
            
            _mapper = new Mapper(new MapperConfiguration(c =>
            {
                c.AddProfile<AccountMappings>();
                c.AddProfile<TransferConnectionInvitationMappings>();
                c.AddProfile<UserMappings>();
            }));

            _handler = new SendTransferConnectionInvitationQueryHandler(
                _employerAccountRepository.Object, 
                _transferConnectionInvitationRepository.Object, 
                _mapper);

            _query = new SendTransferConnectionInvitationQuery
            {
                AccountId = _senderAccount.Id,
                ReceiverAccountPublicHashedId = _receiverAccount.PublicHashedId
            };
        }

        [Test]
        public async Task ThenShouldReturnSendTransferConnectionInvitationResponse()
        {
            _transferConnectionInvitationRepository
                .Setup(s => s.AnyTransferConnectionInvitations(
                    _senderAccount.Id,
                    _receiverAccount.Id,
                    new List<TransferConnectionInvitationStatus> { TransferConnectionInvitationStatus.Pending, TransferConnectionInvitationStatus.Approved }))
                .ReturnsAsync(false);

            _response = await _handler.Handle(_query, CancellationToken.None);

            Assert.That(_response, Is.Not.Null);
            Assert.That(_response.ReceiverAccount, Is.Not.Null);
            Assert.That(_response.ReceiverAccount.Id, Is.EqualTo(_receiverAccount.Id));
        }

        [Test]
        public async Task ThenSenderAccountShouldBePopulatedOnSendTransferConnectionInvitationResponse()
        {
            _transferConnectionInvitationRepository
                .Setup(s => s.AnyTransferConnectionInvitations(
                    _senderAccount.Id,
                    _receiverAccount.Id,
                    new List<TransferConnectionInvitationStatus> { TransferConnectionInvitationStatus.Pending, TransferConnectionInvitationStatus.Approved }))
                .ReturnsAsync(false);

            _response = await _handler.Handle(_query, CancellationToken.None);

            Assert.That(_response, Is.Not.Null);
            Assert.That(_response.SenderAccount, Is.Not.Null);
            Assert.That(_response.SenderAccount.Id, Is.EqualTo(_senderAccount.Id));
        }
        [Test]
        public void ThenShouldThrowValidationExceptionIfReceiverAccountIsNull()
        {
            _employerAccountRepository.Setup(s => s.Get("111111")).ReturnsAsync((Account)null);

            var exception = Assert.ThrowsAsync<ValidationException>( () => _handler.Handle(_query, CancellationToken.None));

            Assert.That(exception.ValidationResult.MemberNames.First().Split("|")[1], Is.EqualTo("You must enter a valid account ID"));
        }

        [Test]
        public void ThenShouldThrowValidationExceptionIfPendingOrApprovedInvitationExists()
        {
            _transferConnectionInvitationRepository
                .Setup(s => s.AnyTransferConnectionInvitations(
                    _senderAccount.Id,
                    _receiverAccount.Id,
                    new List<TransferConnectionInvitationStatus> { TransferConnectionInvitationStatus.Pending, TransferConnectionInvitationStatus.Approved }))
                .ReturnsAsync(true);

            var exception = Assert.ThrowsAsync<ValidationException>(
                () =>  _handler.Handle(_query, CancellationToken.None));

            Assert.That(exception.ValidationResult.MemberNames.First().Split("|")[1], Is.EqualTo("You can't connect with this employer because they already have a pending or accepted connection request"));
        }
    }
}