﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.Courses.Data.UnitTests.DatabaseMock;
using SFA.DAS.EmployerFinance.Data;
using SFA.DAS.EmployerFinance.MarkerInterfaces;
using SFA.DAS.EmployerFinance.Models.Payments;
using SFA.DAS.EmployerFinance.Models.Transaction;
using SFA.DAS.EmployerFinance.Models.Transfers;
using SFA.DAS.EmployerFinance.Queries.GetTransferTransactionDetails;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerFinance.UnitTests.Queries.GetTransferTransactionDetailsTests
{
    [ExcludeFromCodeCoverage]
    class WhenAReceiverGetsTransferTransactionDetails
    {
        private const long SenderAccountId = 1;
        private const string SenderAccountName = "Test Sender";
        private const string SenderPublicHashedId = "ABC123";

        private const long ReceiverAccountId = 2;
        private const string ReceiverAccountName = "Test Receiver";
        private const string ReceiverPublicHashedId = "DEF456";

        private const string PeriodEnd = "1718-R01";

        private const string FirstCourseName = "Course 1";
        private const string SecondCourseName = "Course 2";

        private GetTransferTransactionDetailsQueryHandler _handler;
        private GetTransferTransactionDetailsQuery _query;
        private GetTransferTransactionDetailsResponse _response;
        private Mock<EmployerFinanceDbContext> _db;
        private List<AccountTransfer> _transfers;
        private Mock<IPublicHashingService> _publicHashingService;
        private Mock<IEncodingService> _encodingService;
        private PeriodEnd _periodEnd;
        private TransactionLineEntity _senderTranferTransaction;
        private TransactionLineEntity _recieverTranferTransaction;

        [SetUp]
        public void Assign()
        {
            _db = new Mock<EmployerFinanceDbContext>();

            _periodEnd = new PeriodEnd
            {
                Id = 1,
                PeriodEndId = PeriodEnd,
                AccountDataValidAt = DateTime.Now.AddDays(-2),
                CalendarPeriodMonth = 2,
                CalendarPeriodYear = 2018,
                CommitmentDataValidAt = DateTime.Now.AddDays(-1),
                CompletionDateTime = DateTime.Now,
                PaymentsForPeriod = "Test"
            };

            _senderTranferTransaction = new TransactionLineEntity
            {
                AccountId = SenderAccountId,
                TransferSenderAccountId = SenderAccountId,
                TransferReceiverAccountId = ReceiverAccountId,
                PeriodEnd = PeriodEnd,
                DateCreated = DateTime.Now.AddDays(-2),
                TransactionType = TransactionItemType.Transfer
            };

            _recieverTranferTransaction = new TransactionLineEntity
            {
                AccountId = ReceiverAccountId,
                PeriodEnd = PeriodEnd,
                TransferSenderAccountId = SenderAccountId,
                TransferReceiverAccountId = ReceiverAccountId,
                DateCreated = DateTime.Now.AddDays(-1),
                TransactionType = TransactionItemType.Transfer
            };

            _publicHashingService = new Mock<IPublicHashingService>();

            _encodingService= new Mock<IEncodingService>();

            _query = new GetTransferTransactionDetailsQuery
            {
                AccountId = ReceiverAccountId,
                TargetAccountPublicHashedId = SenderPublicHashedId,
                PeriodEnd = PeriodEnd
            };

            _response = new GetTransferTransactionDetailsResponse();

            _handler = new GetTransferTransactionDetailsQueryHandler(_db.Object, _encodingService.Object);

            _transfers = new List<AccountTransfer>
                {
                    new AccountTransfer
                    {
                        SenderAccountId = SenderAccountId,
                        SenderAccountName = SenderAccountName,
                        ReceiverAccountId = ReceiverAccountId,
                        ReceiverAccountName = ReceiverAccountName,
                        ApprenticeshipId = 1,
                        CourseName = FirstCourseName,
                        Amount = 123.4567M,
                        PeriodEnd = PeriodEnd

                    },
                    new AccountTransfer
                    {
                        SenderAccountId = SenderAccountId,
                        SenderAccountName = SenderAccountName,
                        ReceiverAccountId = ReceiverAccountId,
                        ReceiverAccountName = ReceiverAccountName,
                        ApprenticeshipId = 2,
                        CourseName = SecondCourseName,
                        Amount = 346.789M,
                        PeriodEnd = PeriodEnd
                    },
                    new AccountTransfer
                    {
                        SenderAccountId = SenderAccountId,
                        SenderAccountName = SenderAccountName,
                        ReceiverAccountId = ReceiverAccountId,
                        ReceiverAccountName = ReceiverAccountName,
                        ApprenticeshipId = 3,
                        CourseName = SecondCourseName,
                        Amount = 234.56M,
                        PeriodEnd = PeriodEnd
                    }
                };

            _db.Setup(d => d.SqlQueryAsync<AccountTransfer>(
                It.IsAny<string>(), SenderAccountId, ReceiverAccountId, PeriodEnd))
                .ReturnsAsync(_transfers);

            _db.Setup(d => d.SqlQueryAsync<AccountTransfer>(
                    It.IsAny<string>(), ReceiverAccountId, SenderAccountId, PeriodEnd))
                .ReturnsAsync(_transfers);

            _db.Setup(x => x.PeriodEnds).ReturnsDbSet(new List<PeriodEnd>{_periodEnd});

            _db.Setup(x => x.Transactions).ReturnsDbSet(new List<TransactionLineEntity>{
                _senderTranferTransaction,
                _recieverTranferTransaction});

            _publicHashingService.Setup(x => x.DecodeValue(SenderPublicHashedId))
                .Returns(SenderAccountId);

            _publicHashingService.Setup(x => x.DecodeValue(ReceiverPublicHashedId))
                .Returns(ReceiverAccountId);

            _publicHashingService.Setup(x => x.HashValue(SenderAccountId))
                .Returns(SenderPublicHashedId);

            _publicHashingService.Setup(x => x.HashValue(ReceiverAccountId))
                .Returns(ReceiverPublicHashedId);
        }

        [Test]
        public async Task ThenIShouldReturnCorrectSenderDetails()
        {
            //Act
            var result = await _handler.Handle(_query, CancellationToken.None);

            //Assert
            Assert.AreEqual(SenderAccountName, result.SenderAccountName);
            Assert.AreEqual(SenderPublicHashedId, result.SenderAccountPublicHashedId);
        }

        [Test]
        public async Task ThenIShouldReturnCorrectReceiverDetails()
        {
            //Act
            var result = await _handler.Handle(_query, CancellationToken.None);

            //Assert
            Assert.AreEqual(ReceiverAccountName, result.ReceiverAccountName);
            Assert.AreEqual(ReceiverPublicHashedId, result.ReceiverAccountPublicHashedId);
        }

        [Test]
        public async Task ThenIShouldReturnCorrectCourseDetails()
        {
            //Act
            var result = await _handler.Handle(_query, CancellationToken.None);

            //Assert
            Assert.AreEqual(2, result.TransferDetails.Count());
            Assert.IsTrue(result.TransferDetails.Any(t => t.CourseName.Equals(FirstCourseName)));
            Assert.IsTrue(result.TransferDetails.Any(t => t.CourseName.Equals(SecondCourseName)));
        }

        [Test]
        public async Task ThenIShouldReturnCorrectCourseSubTotals()
        {
            //Act
            var result = await _handler.Handle(_query, CancellationToken.None);

            //Assert
            Assert.AreEqual(2, result.TransferDetails.Count());

            var firstCourseTotal = result.TransferDetails.Single(t => t.CourseName.Equals(FirstCourseName)).PaymentTotal;
            var secondCourseTotal = result.TransferDetails.Single(t => t.CourseName.Equals(SecondCourseName)).PaymentTotal;

            var expectedFirstCourseTotal =
                _transfers.Where(t => t.CourseName.Equals(FirstCourseName)).Sum(x => x.Amount);

            var expectedSecondCourseTotal =
                _transfers.Where(t => t.CourseName.Equals(SecondCourseName)).Sum(x => x.Amount);

            Assert.AreEqual(expectedFirstCourseTotal, firstCourseTotal);
            Assert.AreEqual(expectedSecondCourseTotal, secondCourseTotal);
        }

        [Test]
        public async Task ThenIShouldReturnCorrectCourseApprenticeCount()
        {
            //Act
            var result = await _handler.Handle(_query, CancellationToken.None);

            //Assert

            var firstCourseApprenticeCount = result.TransferDetails.Single(t => t.CourseName.Equals(FirstCourseName))
                .ApprenticeCount;

            var secondCourseApprenticeCount = result.TransferDetails.Single(t => t.CourseName.Equals(SecondCourseName))
                .ApprenticeCount;

            var expectedFirstCourseApprenticeCount =
                _transfers.Count(t => t.CourseName.Equals(FirstCourseName));

            var expectedSecondCourseApprenticeCount =
                _transfers.Count(t => t.CourseName.Equals(SecondCourseName));

            Assert.AreEqual(expectedFirstCourseApprenticeCount, firstCourseApprenticeCount);
            Assert.AreEqual(expectedSecondCourseApprenticeCount, secondCourseApprenticeCount);
        }

        [Test]
        public async Task ThenIShouldReturnTransferPaymentTotal()
        {
            //Act
            var result = await _handler.Handle(_query, CancellationToken.None);

            //Assert
            var expectedPaymentTotal = _transfers.Sum(t => t.Amount);

            Assert.AreEqual(expectedPaymentTotal, result.TransferPaymentTotal);
        }

        [Test]
        public async Task ThenIShouldReturnSenderTransferDate()
        {
            //Assign
            var query = new GetTransferTransactionDetailsQuery
            {
                AccountId = SenderAccountId,
                TargetAccountPublicHashedId = ReceiverPublicHashedId,
                PeriodEnd = PeriodEnd
            };

            //Act
            var result = await _handler.Handle(query, CancellationToken.None);

            //Assert
            Assert.AreEqual(_senderTranferTransaction.DateCreated, result.DateCreated);
        }

        [Test]
        public async Task ThenIShouldReturnReceiverTransferDate()
        {
            //Act
            var result = await _handler.Handle(_query, CancellationToken.None);

            //Assert
            Assert.AreEqual(_recieverTranferTransaction.DateCreated, result.DateCreated);
        }

        [Test]
        public async Task ThenIShouldBeToldIfImTheReceiver()
        {
            //Act
            var result = await _handler.Handle(_query, CancellationToken.None);

            //Assert
            Assert.IsFalse(result.IsCurrentAccountSender);
        }

        [Test]
        public async Task ThenIShouldBeToldIfImTheSender()
        {
            //Arrange
            var query = new GetTransferTransactionDetailsQuery
            {
                AccountId = SenderAccountId,
                TargetAccountPublicHashedId = ReceiverPublicHashedId,
                PeriodEnd = PeriodEnd
            };

            //Act
            var result = await _handler.Handle(query, CancellationToken.None);

            //Assert
            Assert.IsTrue(result.IsCurrentAccountSender);
        }
    }
}
