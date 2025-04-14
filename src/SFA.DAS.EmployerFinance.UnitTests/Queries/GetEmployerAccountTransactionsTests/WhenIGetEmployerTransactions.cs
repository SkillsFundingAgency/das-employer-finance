using System.Diagnostics.CodeAnalysis;
using System.Net;
using SFA.DAS.EmployerFinance.Models.Levy;
using SFA.DAS.EmployerFinance.Models.Payments;
using SFA.DAS.EmployerFinance.Models.Transaction;
using SFA.DAS.EmployerFinance.Models.Transfers;
using SFA.DAS.EmployerFinance.Queries.GetEmployerAccountTransactions;
using SFA.DAS.EmployerFinance.Services.Contracts;
using SFA.DAS.EmployerFinance.Validation;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerFinance.UnitTests.Queries.GetEmployerAccountTransactionsTests;

[ExcludeFromCodeCoverage]
public class WhenIGetEmployerTransactions : QueryBaseTest<GetEmployerAccountTransactionsHandler, GetEmployerAccountTransactionsQuery, GetEmployerAccountTransactionsResponse>
{
    private Mock<IDasLevyService> _dasLevyService;
    private GetEmployerAccountTransactionsQuery _request;
    private Mock<ILogger<GetEmployerAccountTransactionsHandler>> _logger;
    public override GetEmployerAccountTransactionsQuery Query { get; set; }
    public override GetEmployerAccountTransactionsHandler RequestHandler { get; set; }
    public override Mock<IValidator<GetEmployerAccountTransactionsQuery>> RequestValidator { get; set; }
    private Mock<IEncodingService> _encodingService;


    [SetUp]
    public void Arrange()
    {
        SetUp();

        _request = new GetEmployerAccountTransactionsQuery
        {
            HashedAccountId = "RTF34"
        };

        _encodingService = new Mock<IEncodingService>();
        _encodingService.Setup(x => x.Decode(_request.HashedAccountId, EncodingType.AccountId)).Returns(1);

        _dasLevyService = new Mock<IDasLevyService>();
        _dasLevyService.Setup(x => x.GetAccountTransactionsByDateRange(It.IsAny<long>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(Array.Empty<TransactionLine>());

        _dasLevyService.Setup(x => x.GetPreviousAccountTransaction(It.IsAny<long>(), It.IsAny<DateTime>()))
            .ReturnsAsync(2);

        _logger = new Mock<ILogger<GetEmployerAccountTransactionsHandler>>();

        RequestHandler = new GetEmployerAccountTransactionsHandler(
            _dasLevyService.Object,
            RequestValidator.Object,
            _logger.Object,
            _encodingService.Object);
        Query = new GetEmployerAccountTransactionsQuery();
    }

    [Test]
    public override async Task ThenIfTheMessageIsValidTheRepositoryIsCalled()
    {
        //Arrange        
        var daysInMonth = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);

        var fromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        var toDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, daysInMonth);

        _request.FromDate = fromDate;
        _request.ToDate = toDate;

        //Act
        await RequestHandler.Handle(_request, CancellationToken.None);

        //Assert
        _dasLevyService.Verify(x => x.GetAccountTransactionsByDateRange(It.IsAny<long>(), fromDate, toDate), Times.Once);
    }

    [Test]
    public async Task ThenIfAMonthIsProvidedTheRepositoryIsCalledForThatMonthMonth()
    {
        //Arrange
        var fromDate = new DateTime(2017, 3, 1);
        var daysInMonth = DateTime.DaysInMonth(fromDate.Year, fromDate.Month);
        var toDate = new DateTime(fromDate.Year, fromDate.Month, daysInMonth);

        _request.FromDate = fromDate;
        _request.ToDate = toDate;

        //Act
        await RequestHandler.Handle(_request, CancellationToken.None);

        //Assert
        _dasLevyService.Verify(x => x.GetAccountTransactionsByDateRange(It.IsAny<long>(), fromDate, toDate), Times.Once);
    }

    [Test]
    public override async Task ThenIfTheMessageIsValidTheValueIsReturnedInTheResponse()
    {
        //Arrange
        var transactions = new TransactionLine[]
        {
            new LevyDeclarationTransactionLine
            {
                AccountId = 1,
                SubmissionId = 1,
                TransactionDate = DateTime.Now.AddDays(-3),
                Amount = 1000,
                TransactionType = TransactionItemType.TopUp,
                EmpRef = "123"
            }
        };

        _dasLevyService.Setup(x => x.GetAccountTransactionsByDateRange(It.IsAny<long>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(transactions);

        //Act
        var response = await RequestHandler.Handle(_request, CancellationToken.None);

        //Assert
        response.Data.HashedAccountId.Should().Be(_request.HashedAccountId);
        response.Data.AccountId.Should().Be(1);
        response.Data.TransactionLines.Length.Should().Be(1);
    }


    [Test]
    public async Task ThenIfNoTransactionAreFoundAnEmptyTransactionListIsReturned()
    {
        //Act
        var response = await RequestHandler.Handle(_request, CancellationToken.None);

        //Assert
        response.Data.HashedAccountId.Should().Be(_request.HashedAccountId);
        response.Data.AccountId.Should().Be(1);
        response.Data.TransactionLines.Should().BeEmpty();
    }

    [Test]
    public async Task ThenTheProviderNameIsTakenFromTheService()
    {
        //Arrange
        var expectedUkprn = 545646541;
        var transactions = new TransactionLine[]
        {
            new PaymentTransactionLine()
            {
                AccountId = 1,
                TransactionDate = DateTime.Now.AddMonths(-3),
                Amount = 1000,
                TransactionType = TransactionItemType.Payment,
                UkPrn = expectedUkprn,
                PeriodEnd = "17-18"
            }
        };
        _dasLevyService.Setup(x => x.GetAccountTransactionsByDateRange(It.IsAny<long>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(transactions);

        _dasLevyService.Setup(x => x.GetProviderName(expectedUkprn, It.IsAny<long>(), It.IsAny<string>())).ReturnsAsync("test");
        //Act
        var actual = await RequestHandler.Handle(_request, CancellationToken.None);

        //Assert
        _dasLevyService.Verify(x => x.GetProviderName(expectedUkprn, It.IsAny<long>(), It.IsAny<string>()), Times.Once);
    }

    [Test]
    public async Task ThenTheProviderNameIsNotRecognisedIfTheRecordThrowsAndException()
    {
        //Arrange
        var transactions = new TransactionLine[]
        {
            new PaymentTransactionLine
            {
                AccountId = 1,
                TransactionDate = DateTime.Now.AddMonths(-3),
                Amount = 1000,
                TransactionType = TransactionItemType.Payment,
                UkPrn = 1254545
            }
        };
        _dasLevyService.Setup(x => x.GetAccountTransactionsByDateRange(It.IsAny<long>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(transactions);

        _dasLevyService.Setup(x => x.GetProviderName(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<string>())).Throws(new WebException());

        //Act
        var actual = await RequestHandler.Handle(_request, CancellationToken.None);

        //Assert
        actual.Data.TransactionLines.First().Description.Should().Be("Training provider - name not recognised");
        _logger.Verify(x => x.Log(LogLevel.Information, 0,
            It.Is<It.IsAnyType>((message, type) => message.ToString().StartsWith("Provider not found for UkPrn:1254545")),
            It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()
        ), Times.Once);
    }

    [Test]
    public async Task ThenTheProviderNameIsSetToUnknownProviderIfTheRecordCantBeFound()
    {
        //Arrange
        var transactions = new TransactionLine[]
        {
            new PaymentTransactionLine
            {
                AccountId = 1,
                TransactionDate = DateTime.Now.AddMonths(-3),
                Amount = 1000,
                TransactionType = TransactionItemType.Payment,
                UkPrn = 1254545
            }
        };
        _dasLevyService.Setup(x => x.GetAccountTransactionsByDateRange(It.IsAny<long>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(transactions);

        _dasLevyService.Setup(x => x.GetProviderName(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<string>()))
            .ReturnsAsync((string)null);

        //Act
        var actual = await RequestHandler.Handle(_request, CancellationToken.None);

        //Assert
        actual.Data.TransactionLines.First().Description.Should().Be("Training provider - name not recognised");
    }

    [Test]
    public async Task ThenShouldLogIfExceptionOccursWhenGettingProviderName()
    {
        //Arrange
        var transactions = new TransactionLine[]
        {
            new PaymentTransactionLine
            {
                AccountId = 1,
                TransactionDate = DateTime.Now.AddMonths(-3),
                Amount = 1000,
                TransactionType = TransactionItemType.Payment,
                UkPrn = 1254545
            }
        };

        _dasLevyService.Setup(x => x.GetAccountTransactionsByDateRange(It.IsAny<long>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(transactions);

        _dasLevyService.Setup(x => x.GetProviderName(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<string>()))
            .Throws<Exception>();

        //Act
        await RequestHandler.Handle(_request, CancellationToken.None);

        //Assert
        _logger.Verify(x => x.Log(LogLevel.Information, 0,
            It.Is<It.IsAnyType>((message, type) => message.ToString().StartsWith("Provider not found for UkPrn:1254545")),
            It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()
        ), Times.Once);
    }

    [Test]
    public async Task ThenIShouldGetBackCorrectLevyTransactions()
    {
        //Arrange
        var transaction = new LevyDeclarationTransactionLine
        {
            TransactionType = TransactionItemType.Declaration,
            Amount = 123.45M
        };

        _dasLevyService.Setup(x => x.GetAccountTransactionsByDateRange(It.IsAny<long>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new TransactionLine[]
            {
                transaction
            });

        //Act
        var actual = await RequestHandler.Handle(Query, CancellationToken.None);

        //Assert
        var actualTransaction = actual.Data.TransactionLines.First();

        actualTransaction.Description.Should().Be("Levy");
        actualTransaction.Amount.Should().Be(transaction.Amount);
    }


    [Test]
    public async Task ThenIShouldGetBackCorrectLevyAdjustmentTransactions()
    {
        //Arrange
        var transaction = new LevyDeclarationTransactionLine
        {
            TransactionType = TransactionItemType.Declaration,
            Amount = -100.50M
        };

        _dasLevyService.Setup(x => x.GetAccountTransactionsByDateRange(It.IsAny<long>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new TransactionLine[]
            {
                transaction
            });

        //Act
        var actual = await RequestHandler.Handle(Query, CancellationToken.None);

        //Assert
        var actualTransaction = actual.Data.TransactionLines.First();

        actualTransaction.Description.Should().Be("Levy adjustment");
        actualTransaction.Amount.Should().Be(transaction.Amount);
    }

    [Test]
    public async Task ThenIShouldGetBackCorrectPaymentTransactions()
    {
        //Arrange
        var provider = new EmployerFinance.Models.ApprenticeshipProvider.Provider { Name = "test" };
        var transaction = new PaymentTransactionLine
        {
            UkPrn = 100,
            TransactionType = TransactionItemType.Payment,
            Amount = 123.45M
        };

        _dasLevyService.Setup(x => x.GetAccountTransactionsByDateRange(It.IsAny<long>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new TransactionLine[]
            {
                transaction
            });

        _dasLevyService.Setup(x => x.GetProviderName(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<string>()))
            .ReturnsAsync(provider.Name);

        //Act
        var actual = await RequestHandler.Handle(Query, CancellationToken.None);

        //Assert
        var actualTransaction = actual.Data.TransactionLines.First();

        actualTransaction.Description.Should().Be(provider.Name);
        actualTransaction.Amount.Should().Be(transaction.Amount);
    }

    [Test]
    public async Task ThenIShouldGetBackCorrectCoInvestmentTransactionFromSFAPayment()
    {
        //Arrange
        var provider = new EmployerFinance.Models.ApprenticeshipProvider.Provider { Name = "test" };
        var transaction = new PaymentTransactionLine
        {
            UkPrn = 100,
            TransactionType = TransactionItemType.Payment,
            SfaCoInvestmentAmount = 123.45M
        };

        _dasLevyService.Setup(x => x.GetAccountTransactionsByDateRange(It.IsAny<long>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new TransactionLine[]
            {
                transaction
            });

        _dasLevyService.Setup(x => x.GetProviderName(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<string>()))
            .ReturnsAsync(provider.Name);

        //Act
        var actual = await RequestHandler.Handle(Query, CancellationToken.None);

        //Assert
        var actualTransaction = actual.Data.TransactionLines.First();

        actualTransaction.Description.Should().Be($"Co-investment - {provider.Name}");
        actualTransaction.Amount.Should().Be(transaction.Amount);
    }

    [Test]
    public async Task ThenIShouldGetBackCorrectCoInvestmentTransactionFromEmployerPayment()
    {
        //Arrange
        var provider = new EmployerFinance.Models.ApprenticeshipProvider.Provider { Name = "test" };
        var transaction = new PaymentTransactionLine
        {
            UkPrn = 100,
            TransactionType = TransactionItemType.Payment,
            Amount = 123.45M,
            EmployerCoInvestmentAmount = 50
        };

        _dasLevyService.Setup(x => x.GetAccountTransactionsByDateRange(It.IsAny<long>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new TransactionLine[]
            {
                transaction
            });

        _dasLevyService.Setup(x => x.GetProviderName(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<string>()))
            .ReturnsAsync(provider.Name);

        //Act
        var actual = await RequestHandler.Handle(Query, CancellationToken.None);

        //Assert
        var actualTransaction = actual.Data.TransactionLines.First();

        actualTransaction.Description.Should().Be($"Co-investment - {provider.Name}");
        actualTransaction.Amount.Should().Be(transaction.Amount);
    }

    [Test]
    public async Task ThenShouldReturnPreviousTransactionsAreAvailableIfThereAreSome()
    {
        //Arrange
        var transactions = new TransactionLine[]
        {
            new LevyDeclarationTransactionLine
            {
                AccountId = 1,
                SubmissionId = 1,
                TransactionDate = DateTime.Now.AddDays(-3),
                Amount = 1000,
                TransactionType = TransactionItemType.TopUp,
                EmpRef = "123"
            }
        };

        _dasLevyService.Setup(x => x.GetAccountTransactionsByDateRange(It.IsAny<long>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(transactions);

        //Act
        var result = await RequestHandler.Handle(_request, CancellationToken.None);

        //Assert
        result.AccountHasPreviousTransactions.Should().BeTrue();
    }

    [Test]
    public async Task ThenShouldReturnPreviousTransactionsAreNotAvailableIfThereAreNone()
    {
        //Arrange
        var transactions = new TransactionLine[]
        {
            new LevyDeclarationTransactionLine
            {
                AccountId = 1,
                SubmissionId = 1,
                TransactionDate = DateTime.Now.AddDays(-3),
                Amount = 1000,
                TransactionType = TransactionItemType.TopUp,
                EmpRef = "123"
            }
        };

        _dasLevyService.Setup(x => x.GetAccountTransactionsByDateRange(It.IsAny<long>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(transactions);

        _dasLevyService.Setup(x => x.GetPreviousAccountTransaction(It.IsAny<long>(), It.IsAny<DateTime>()))
            .ReturnsAsync(0);

        //Act
        var result = await RequestHandler.Handle(_request, CancellationToken.None);

        //Assert
        result.AccountHasPreviousTransactions.Should().BeFalse();
    }

    [Test]
    public async Task ThenIShouldGetBackCorrectTransferTransactions()
    {
        //Arrange
        var transaction = new TransferTransactionLine
        {
            ReceiverAccountName = "Test Corp",
            TransactionType = TransactionItemType.Transfer,
            Amount = 2035.20M
        };

        var expectedDescription = $"Transfer sent to {transaction.ReceiverAccountName}";

        _dasLevyService.Setup(x =>
                x.GetAccountTransactionsByDateRange(It.IsAny<long>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new TransactionLine[]
            {
                transaction
            });

        //Act
        var actual = await RequestHandler.Handle(Query, CancellationToken.None);

        //Assert
        var actualTransaction = actual.Data.TransactionLines.First();

        actualTransaction.Description.Should().Be(expectedDescription);
        actualTransaction.Amount.Should().Be(transaction.Amount);
    }

    [Test]
    public async Task ThenIShouldGetTransferReceiverPublicHashedId()
    {
        //Arrange
        var expectedPublicHashedId = "TTT222";

        var transaction = new TransferTransactionLine
        {
            ReceiverAccountId = 3,
            ReceiverAccountName = "Test Corp",
            TransactionType = TransactionItemType.Transfer,
            Amount = 2035.20M
        };

        _dasLevyService.Setup(x =>
                x.GetAccountTransactionsByDateRange(It.IsAny<long>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new TransactionLine[]
            {
                transaction
            });

        _encodingService.Setup(x => x.Encode(transaction.ReceiverAccountId, EncodingType.PublicAccountId))
            .Returns(expectedPublicHashedId);

        //Act
        var actual = await RequestHandler.Handle(Query, CancellationToken.None);

        //Assert
        var actualTransaction = actual.Data.TransactionLines.First() as TransferTransactionLine;

        actualTransaction?.ReceiverAccountPublicHashedId.Should().Be(expectedPublicHashedId);
    }

    [Test]
    public async Task ThenIShouldGetBackCorrectExpiredFundTransactions()
    {
        //Arrange
        var transaction = new ExpiredFundTransactionLine
        {
            TransactionType = TransactionItemType.ExpiredFund,
            Amount = 2035.20M
        };

        var expectedDescription = "Expired levy";

        _dasLevyService.Setup(x =>
                x.GetAccountTransactionsByDateRange(It.IsAny<long>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new TransactionLine[]
            {
                transaction
            });

        //Act
        var actual = await RequestHandler.Handle(Query, CancellationToken.None);

        //Assert
        var actualTransaction = actual.Data.TransactionLines.First();

        actualTransaction.Description.Should().Be(expectedDescription);
        actualTransaction.Amount.Should().Be(transaction.Amount);
    }
}