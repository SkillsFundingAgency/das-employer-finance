using System.ComponentModel.DataAnnotations;
using SFA.DAS.EmployerFinance.Api.Controllers;
using SFA.DAS.EmployerFinance.Api.Orchestrators;
using SFA.DAS.EmployerFinance.Api.Types;
using SFA.DAS.EmployerFinance.Commands.PersistLevyDeclarations;

namespace SFA.DAS.EmployerFinance.Api.UnitTests.Controllers.FinanceLevyDeclarationsControllerTests;

public class WhenIPersistLevyDeclarations
{
    private FinanceLevyDeclarationsController _controller = null!;
    private Mock<IMediator> _mediator = null!;
    private Mock<ILogger<LevyDeclarationOrchestrator>> _logger = null!;

    [SetUp]
    public void Arrange()
    {
        _mediator = new Mock<IMediator>();
        _logger = new Mock<ILogger<LevyDeclarationOrchestrator>>();
        var orchestrator = new LevyDeclarationOrchestrator(_mediator.Object, _logger.Object);
        _controller = new FinanceLevyDeclarationsController(orchestrator);
    }

    [Test]
    public async Task Then_Returns_BadRequest_When_Request_Is_Null()
    {
        var result = await _controller.Persist(null!) as BadRequestObjectResult;

        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(400);
        result.Value.Should().Be("Request payload is required.");
    }

    [Test]
    public async Task Then_Returns_BadRequest_When_Validation_Fails()
    {
        _mediator
            .Setup(x => x.Send(It.IsAny<PersistLevyDeclarationsCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException(
                new System.ComponentModel.DataAnnotations.ValidationResult(
                    "Validation failed",
                    ["EmpRef|EmpRef has not been supplied."]),
                null,
                null));

        var result = await _controller.Persist(new PersistLevyDeclarationRequestData()) as BadRequestObjectResult;

        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(400);
        var errors = result.Value as Dictionary<string, string>;
        errors.Should().NotBeNull();
        errors!["EmpRef"].Should().Be("EmpRef has not been supplied.");
    }

    [Test]
    public async Task Then_Returns_Ok_With_Response()
    {
        var request = new PersistLevyDeclarationRequestData
        {
            AccountId = 99,
            EmpRef = "123/REF",
            Declarations =
            [
                new NormalizedLevyDeclaration
                {
                    Id = 1,
                    SubmissionId = 2,
                    PayrollYear = "25-26",
                    PayrollMonth = 1,
                    SubmissionDate = new DateTime(2026, 5, 1)
                }
            ]
        };

        var expected = new PersistLevyDeclarationsResponse
        {
            DeclarationsPersisted = 1,
            DeclarationsSkipped = 0,
            TransactionsCreated = 1
        };

        _mediator
            .Setup(x => x.Send(It.Is<PersistLevyDeclarationsCommand>(c => c.Data == request), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.Persist(request) as OkObjectResult;

        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(200);
        result.Value.Should().BeEquivalentTo(expected);
    }
}
