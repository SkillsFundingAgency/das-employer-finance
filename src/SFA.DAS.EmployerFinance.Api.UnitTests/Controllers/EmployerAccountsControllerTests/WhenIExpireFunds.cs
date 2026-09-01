using System.ComponentModel.DataAnnotations;
using System.Reflection;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.EmployerFinance.Api.Authorization;
using SFA.DAS.EmployerFinance.Api.Controllers;
using SFA.DAS.EmployerFinance.Api.Orchestrators;
using SFA.DAS.EmployerFinance.Api.Types;
using SFA.DAS.EmployerFinance.Commands.ExpireAccountFunds;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerFinance.Api.UnitTests.Controllers.EmployerAccountsControllerTests;

public class WhenIExpireFunds
{
    private EmployerAccountsController _controller = null!;
    private Mock<IMediator> _mediator = null!;

    [SetUp]
    public void Arrange()
    {
        _mediator = new Mock<IMediator>();
        var orchestrator = new FinanceOrchestrator(
            _mediator.Object,
            Mock.Of<ILogger<FinanceOrchestrator>>(),
            Mock.Of<IMapper>(),
            Mock.Of<IEncodingService>());

        _controller = new EmployerAccountsController(orchestrator);
    }

    [Test]
    public async Task Then_Returns_BadRequest_When_Request_Is_Null()
    {
        var result = await _controller.ExpireFunds(123, null) as BadRequestObjectResult;

        result.Should().NotBeNull();
        result!.Value.Should().Be("Expire funds payload is required.");
        _mediator.Verify(
            mediator => mediator.Send(
                It.IsAny<ExpireAccountFundsCommand>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task Then_Returns_BadRequest_With_Validation_Errors()
    {
        _mediator
            .Setup(mediator => mediator.Send(
                It.IsAny<ExpireAccountFundsCommand>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException(
                new System.ComponentModel.DataAnnotations.ValidationResult(
                    "Validation failed",
                    ["AccountId|AccountId must be greater than 0."]),
                null,
                null));

        var result = await _controller.ExpireFunds(
            0,
            new ExpireFundsRequest { CorrelationId = "corr-123" }) as BadRequestObjectResult;

        result.Should().NotBeNull();
        var errors = result!.Value as Dictionary<string, string>;
        errors.Should().NotBeNull();
        errors!["AccountId"].Should().Be("AccountId must be greater than 0.");
    }

    [Test]
    public async Task Then_Passes_The_Route_Account_And_CorrelationId_And_Returns_Ok()
    {
        var expectedResponse = new ExpireFundsResponse
        {
            AccountId = 123,
            CorrelationId = "corr-123",
            FundsExpired = true,
            LongTermExpiredFundsCount = 2,
            ShortTermExpiredFundsCount = 1
        };

        _mediator
            .Setup(mediator => mediator.Send(
                It.Is<ExpireAccountFundsCommand>(command =>
                    command.AccountId == 123
                    && command.CorrelationId == "corr-123"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var result = await _controller.ExpireFunds(
            123,
            new ExpireFundsRequest { CorrelationId = "corr-123" }) as OkObjectResult;

        result.Should().NotBeNull();
        result!.Value.Should().BeSameAs(expectedResponse);
    }

    [Test]
    public void Then_Unexpected_Errors_Are_Not_Swallowed()
    {
        var expectedException = new InvalidOperationException("Finance database unavailable");
        _mediator
            .Setup(mediator => mediator.Send(
                It.IsAny<ExpireAccountFundsCommand>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        var exception = Assert.ThrowsAsync<InvalidOperationException>(() =>
            _controller.ExpireFunds(
                123,
                new ExpireFundsRequest { CorrelationId = "corr-123" }));

        exception.Should().BeSameAs(expectedException);
    }

    [Test]
    public void Then_The_Endpoint_Has_The_Expected_Route_And_Authorization()
    {
        var action = typeof(EmployerAccountsController).GetMethod(
            nameof(EmployerAccountsController.ExpireFunds));

        action.Should().NotBeNull();
        action!.GetCustomAttribute<HttpPostAttribute>()!.Template
            .Should().Be("{accountId}/expire-funds");
        action.GetCustomAttribute<AuthorizeAttribute>()!.Policy
            .Should().Be(ApiRoles.ReadAllEmployerAccountBalances);
    }
}
