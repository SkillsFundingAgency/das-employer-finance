using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerFinance.Formatters.TransactionDowloads;
using SFA.DAS.EmployerFinance.Messages;
using SFA.DAS.EmployerFinance.Models.Transaction;
using SFA.DAS.EmployerFinance.Queries.GetTransactionsDownload;
using SFA.DAS.EmployerFinance.Web.Controllers;
using SFA.DAS.EmployerFinance.Web.Orchestrators;
using SFA.DAS.EmployerFinance.Web.ViewModels;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerFinance.Web.UnitTests.Controllers.EmployerAccountTransactionsControllerTests;

[TestFixture]
public class WhenIDownloadTransactionsByDate
{
    private const string ExpectedFileExtension = "hello";
    private const string ExpectedMimeType = @"text/csv";
    private const string HashedAccountId = "ABC123";
    private const long AccountId = 324324;
    private static readonly byte[] ExpectedFileData = { };

    private EmployerAccountTransactionsController _controller;
    private Mock<IMediator> _mediator;
    private Mock<IEmployerAccountTransactionsOrchestrator> _orchestrator;
    private Mock<ITransactionFormatter> _formatter;
    private TransactionDownloadViewModel _transactionDownloadViewModel;
    private Mock<IEncodingService> _encodingService;

    [SetUp]
    public void Arrange()
    {
        _transactionDownloadViewModel = new TransactionDownloadViewModel
        {
            
            StartDate = new MonthYear
            {
                Month = "1",
                Year = "2000"
            },
            EndDate = new MonthYear
            {
                Month = "1",
                Year = "2018"
            }
        
        };

            _mediator = new Mock<IMediator>();
            _orchestrator = new Mock<IEmployerAccountTransactionsOrchestrator>();
            _formatter = new Mock<ITransactionFormatter>();
            _encodingService = new Mock<IEncodingService>();
            _encodingService.Setup(x => x.Decode(HashedAccountId, EncodingType.AccountId)).Returns(AccountId);
            _mediator.Setup(m => m.Send(It.IsAny<GetTransactionsDownloadQuery>(), CancellationToken.None))
                .ReturnsAsync(new GetTransactionsDownloadResponse
                {
                    MimeType = ExpectedMimeType,
                    FileExtension = ExpectedFileExtension,
                    FileData = ExpectedFileData
                });

        _formatter.Setup(x => x.GetFileData(It.IsAny<List<TransactionDownloadLine>>())).Returns(new byte[] { 1, 2, 3, 4 });
        _formatter.Setup(x => x.MimeType).Returns("txt/csv");
        _formatter.Setup(x => x.DownloadFormatType).Returns(DownloadFormatType.CSV);

            _controller = new EmployerAccountTransactionsController(
                _orchestrator.Object,
                Mock.Of<IMapper>(),
                _mediator.Object, _encodingService.Object);
        }

    [Test]
    public async Task ThenAGetTransactionsDownloadQueryShouldBeSent()
    {
        await _controller.TransactionsDownload(HashedAccountId, _transactionDownloadViewModel);

        _mediator.Verify(m => m.Send(It.Is<GetTransactionsDownloadQuery>(c=>
            c.AccountId.Equals(AccountId)
            && c.EndDate.Equals(_transactionDownloadViewModel.EndDate)
            && c.StartDate.Equals(_transactionDownloadViewModel.StartDate)
            && c.DownloadFormat.Equals(_transactionDownloadViewModel.DownloadFormat)
            ), CancellationToken.None), Times.Once);
    }

    [Test]
    public async Task ThenTheModelStateShouldBeValid()
    {
        await _controller.TransactionsDownload(HashedAccountId, _transactionDownloadViewModel);

    Assert.That(_controller.ModelState.IsValid, Is.True);
}

    [Test]
    public async Task ThenIShouldBeRedirectedToTheSendTransferConnectionInvitationPage()
    {
        var result =
            await _controller.TransactionsDownload(HashedAccountId, _transactionDownloadViewModel) as
                FileContentResult;

        Assert.That(result, Is.Not.Null);
        Assert.AreEqual(result.ContentType, ExpectedMimeType);
        Assert.AreEqual(result.FileContents, ExpectedFileData);
        Assert.IsTrue(result.FileDownloadName.EndsWith(ExpectedFileExtension));
    }

    [Test]
    public async Task ThenIfThereIsAValidationExceptionItIsHanded()
    {
        _mediator.Setup(m => m.Send(It.IsAny<GetTransactionsDownloadQuery>(), CancellationToken.None))
            .ThrowsAsync(new ValidationException());

        var result =
            await _controller.TransactionsDownload(HashedAccountId, _transactionDownloadViewModel) as ViewResult;
        
        Assert.IsNotNull(result);
    }
    
}