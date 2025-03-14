﻿using System.ComponentModel.DataAnnotations;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.EAS.Account.Api.Types;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Formatters.TransactionDowloads;
using SFA.DAS.EmployerFinance.Formatters.TransactionDowloads.Csv;
using SFA.DAS.EmployerFinance.Interfaces;
using SFA.DAS.EmployerFinance.Models.Transaction;
using SFA.DAS.EmployerFinance.Queries.GetTransactionsDownload;
using MonthYear = SFA.DAS.EmployerFinance.Messages.MonthYear;

namespace SFA.DAS.EmployerFinance.UnitTests.Queries.GetTransactionsDownloadTests;

[TestFixture]
public class WhenIGetTransactionsDownload
{
    private const long AccountId = 111111;
    private static readonly MonthYear StartDate = new MonthYear { Month = "1", Year = "2000" };
    private static readonly MonthYear EndDate = new MonthYear { Month = "1", Year = "2000" };
    private static readonly DateTime ToDate = new DateTime(2000, 2, 1);

    private GetTransactionsDownloadQueryHandler _handler;
    private GetTransactionsDownloadQuery _query;
    private Mock<ITransactionFormatterFactory> _transactionFormatterFactory;
    private Mock<ITransactionRepository> _transactionsRepository;
    private Mock<IAccountApiClient> _accountApiClientMock;

    [SetUp]
    public void SetUp()
    {
        _accountApiClientMock = new Mock<IAccountApiClient>();
        _transactionsRepository = new Mock<ITransactionRepository>();
        _transactionFormatterFactory = new Mock<ITransactionFormatterFactory>();

        _transactionFormatterFactory
            .Setup(x => x.GetTransactionsFormatterByType(It.IsAny<DownloadFormatType>(), It.IsAny<ApprenticeshipEmployerType>()))
            .Returns(new LevyCsvTransactionFormatter());

        _transactionsRepository.Setup(x => x.GetAllTransactionDetailsForAccountByDate(AccountId, StartDate.ToDate(), ToDate))
            .ReturnsAsync(new TransactionDownloadLine[] { new TransactionDownloadLine() { TransactionType = "Levy" } });

        _accountApiClientMock
            .Setup(mock => mock.GetAccount(AccountId))
            .ReturnsAsync(new AccountDetailViewModel
            {
                ApprenticeshipEmployerType = ApprenticeshipEmployerType.Levy.ToString()
            }); ;

        _handler = new GetTransactionsDownloadQueryHandler(_transactionFormatterFactory.Object, _transactionsRepository.Object, _accountApiClientMock.Object);

        _query = new GetTransactionsDownloadQuery
        {
            AccountId = AccountId,
            DownloadFormat = DownloadFormatType.CSV,
            StartDate = StartDate,
            EndDate = EndDate
        };
    }

    [Test]
    public async Task ThenShouldGetATransactionsFormatter()
    {
        await _handler.Handle(_query, CancellationToken.None);

        _transactionFormatterFactory.Verify(x => x.GetTransactionsFormatterByType(_query.DownloadFormat.Value, ApprenticeshipEmployerType.Levy), Times.Once());
    }

    [Test]
    public async Task ThenShouldReturnValidResponse()
    {
        var response = await _handler.Handle(_query, CancellationToken.None);

        response.Should().NotBeNull();
        response.FileExtension.Should().Be("csv");
        response.MimeType.Should().Be("text/csv");
        response.FileData.Should().NotBeNull();
    }

    [Test]
    public void TheShouldThrowValidationExceptionIfNoTransactionFound()
    {
        _transactionsRepository.Setup(r => r.GetAllTransactionDetailsForAccountByDate(AccountId, StartDate, ToDate))
            .ReturnsAsync(Array.Empty<TransactionDownloadLine>());

        Assert.ThrowsAsync<ValidationException>(async () => await _handler.Handle(_query, CancellationToken.None), "There are no transactions in the date range");
    }
}