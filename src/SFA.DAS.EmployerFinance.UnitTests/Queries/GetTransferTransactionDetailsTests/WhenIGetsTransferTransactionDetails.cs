﻿using System.Diagnostics.CodeAnalysis;
using SFA.DAS.EmployerFinance.Data;
using SFA.DAS.EmployerFinance.Models.Payments;
using SFA.DAS.EmployerFinance.Models.Transaction;
using SFA.DAS.EmployerFinance.Models.Transfers;
using SFA.DAS.EmployerFinance.Queries.GetTransferTransactionDetails;
using SFA.DAS.EmployerFinance.TestCommon.DatabaseMock;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerFinance.UnitTests.Queries.GetTransferTransactionDetailsTests;

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
    private Mock<EmployerFinanceDbContext> _db;
    private List<AccountTransfer> _transfers;
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


        _encodingService= new Mock<IEncodingService>();

        _query = new GetTransferTransactionDetailsQuery
        {
            AccountId = ReceiverAccountId,
            TargetAccountPublicHashedId = SenderPublicHashedId,
            PeriodEnd = PeriodEnd
        };

        _handler = new GetTransferTransactionDetailsQueryHandler(new Lazy<EmployerFinanceDbContext>(() => _db.Object), _encodingService.Object, Mock.Of<ILogger<GetTransferTransactionDetailsQueryHandler>>());

        _transfers = new List<AccountTransfer>
        {
            new AccountTransfer
            {
                SenderAccountId = SenderAccountId,
                SenderAccountName = SenderAccountName,
                ReceiverAccountId = ReceiverAccountId,
                ReceiverAccountName = ReceiverAccountName,
                ApprenticeshipId = 1,
                CourseName = "Unknown Course",
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

        var payments = new List<Payment>
        {
            new()
            {
                Id = Guid.NewGuid(),
                ApprenticeshipId = 1,
                EmployerAccountId = ReceiverAccountId,
                CollectionPeriodId = PeriodEnd,
                PaymentMetaDataId = 222
            },
            new()
            {
                Id = Guid.NewGuid(),
                ApprenticeshipId = 2,
                EmployerAccountId = ReceiverAccountId,
                CollectionPeriodId = PeriodEnd,
                PaymentMetaDataId = 333
            },
            new()
            {
                Id = Guid.NewGuid(),
                ApprenticeshipId = 3,
                EmployerAccountId = ReceiverAccountId,
                CollectionPeriodId = PeriodEnd,
                PaymentMetaDataId = 444
            }
        };

        var paymentMetadata = new List<PaymentMetaData>
        {
            new()
            {
                Id = 222,
                ApprenticeshipCourseName = FirstCourseName,
                ApprenticeshipCourseLevel = 6
            },
            new()
            {
                Id = 333,
                ApprenticeshipCourseName = SecondCourseName,
                ApprenticeshipCourseLevel = 7
            },
            new()
            {
                Id = 444,
                ApprenticeshipCourseName = SecondCourseName,
                ApprenticeshipCourseLevel = 7
            }
        };

        _db.Setup(x => x.PaymentMetaData).ReturnsDbSet(paymentMetadata);

        _db.Setup(x => x.Payments).ReturnsDbSet(payments);

        _db.Setup(x => x.AccountTransfers).ReturnsDbSet(_transfers);

        _db.Setup(x => x.PeriodEnds).ReturnsDbSet(new List<PeriodEnd>{_periodEnd});

        _db.Setup(x => x.Transactions).ReturnsDbSet(new List<TransactionLineEntity>{
            _senderTranferTransaction,
            _recieverTranferTransaction});

        _encodingService.Setup(x => x.Decode(SenderPublicHashedId, EncodingType.PublicAccountId))
            .Returns(SenderAccountId);

        _encodingService.Setup(x => x.Decode(ReceiverPublicHashedId, EncodingType.PublicAccountId))
            .Returns(ReceiverAccountId);

        _encodingService.Setup(x => x.Encode(SenderAccountId, EncodingType.PublicAccountId))
            .Returns(SenderPublicHashedId);

        _encodingService.Setup(x => x.Encode(ReceiverAccountId, EncodingType.PublicAccountId))
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
        result.TransferDetails.Should().HaveCount(2);
        result.TransferDetails.ToList()[0].CourseName.Should().Be(FirstCourseName);
        result.TransferDetails.ToList()[1].CourseName.Should().Be(SecondCourseName);
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

        firstCourseTotal.Should().Be(123.4567M);
        secondCourseTotal.Should().Be(581.349M);
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
        
        var expectedSecondCourseApprenticeCount =
            _transfers.Count(t => t.CourseName.Equals(SecondCourseName));

        Assert.AreEqual(1, firstCourseApprenticeCount);
        Assert.AreEqual(2, secondCourseApprenticeCount);
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