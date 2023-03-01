﻿using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerFinance.Web.Controllers;
using SFA.DAS.EmployerFinance.Web.Orchestrators;
using SFA.DAS.EmployerFinance.Web.ViewModels;

namespace SFA.DAS.EmployerFinance.Web.UnitTests.Controllers.EmployerAccountTransactionsControllerTests;

public class WhenIViewFinanceDashboard
{
    private const string ExpectedHashedAccountId = "ABC123";
    private const decimal ExpectedCurrentFunds = 123.45M;

    private EmployerAccountTransactionsController _controller;
    private Mock<IEmployerAccountTransactionsOrchestrator> _orchestrator;
        
    [SetUp]
    public void Arrange()
    {
        _orchestrator = new Mock<IEmployerAccountTransactionsOrchestrator>();
        _orchestrator.Setup(o => o.Index(ExpectedHashedAccountId))
            .ReturnsAsync(new Web.Orchestrators.OrchestratorResponse<FinanceDashboardViewModel>
            {
                Data = new FinanceDashboardViewModel
                {
                    AccountHashedId = ExpectedHashedAccountId,
                    CurrentLevyFunds = ExpectedCurrentFunds
                }
            });

        _controller = new EmployerAccountTransactionsController(
            _orchestrator.Object,
            Mock.Of<IMapper>(),
            Mock.Of<IMediator>(),
            Mock.Of<ILogger<EmployerAccountTransactionsController>>());
    }

    [Test]
    public async Task ThenTheAccountHashedIdIsReturned()
    {
        //Act
        var result = await _controller.Index(ExpectedHashedAccountId);

        //Assert
        var viewResult = result as ViewResult;
        Assert.IsNotNull(viewResult);

        var model = viewResult.Model as Web.Orchestrators.OrchestratorResponse<FinanceDashboardViewModel>;
        Assert.IsNotNull(model);
        Assert.IsNotNull(model.Data);
        Assert.AreEqual(ExpectedHashedAccountId, model.Data.AccountHashedId);
    }

    [Test]
    public async Task ThenTheViewModelHasTheCorrectLevyBalance()
    {
        //Act
        var result = await _controller.Index(ExpectedHashedAccountId);

        //Assert
        var viewResult = result as ViewResult;
        Assert.IsNotNull(viewResult);

        var model = viewResult.Model as Web.Orchestrators.OrchestratorResponse<FinanceDashboardViewModel>;
        Assert.IsNotNull(model);
        Assert.IsNotNull(model.Data);
        Assert.AreEqual(ExpectedCurrentFunds, model.Data.CurrentLevyFunds);
    }

    [Test]
    public async Task ThenCorrectRedirectResultIsReturnedWhenOrchestratorRequestARedirect()
    {
        //Arrange
        const string redirectUrl = "http://example.com";

        _orchestrator.Setup(o => o.Index(It.IsAny<string>()))
            .ReturnsAsync(new Web.Orchestrators.OrchestratorResponse<FinanceDashboardViewModel>
            {
                RedirectUrl = redirectUrl
            });

        //Act
        var result = await _controller.Index(ExpectedHashedAccountId);

        //Assert
        var redirectResult = result as RedirectResult;
        Assert.IsNotNull(redirectResult);
        Assert.AreEqual(redirectUrl, redirectResult.Url);
        Assert.IsFalse(redirectResult.Permanent);
    }

    [Test]
    public async Task ThenRedirectResultIsNotReturnedWhenOrchestratorDoesNotRequestARedirect()
    {
        //Act
        var result = await _controller.Index(ExpectedHashedAccountId);

        //Assert
        Assert.IsNotInstanceOf<RedirectResult>(result);
    }
}