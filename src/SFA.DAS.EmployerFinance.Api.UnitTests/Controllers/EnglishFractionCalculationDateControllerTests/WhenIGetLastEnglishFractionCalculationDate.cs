using System.ComponentModel.DataAnnotations;
using SFA.DAS.EmployerFinance.Api.Controllers;
using SFA.DAS.EmployerFinance.Api.Orchestrators;
using SFA.DAS.EmployerFinance.Api.Types;
using SFA.DAS.EmployerFinance.Queries.GetLastEnglishFractionCalculationDate;

namespace SFA.DAS.EmployerFinance.Api.UnitTests.Controllers.EnglishFractionCalculationDateControllerTests;

public class WhenIGetLastEnglishFractionCalculationDate
{
    private EnglishFractionCalculationDateController _controller;
    private Mock<IMediator> _mediator;

    [SetUp]
    public void Arrange()
    {
        _mediator = new Mock<IMediator>();
        var logger = new Mock<ILogger<EnglishFractionCalculationDateOrchestrator>>();
        var orchestrator = new EnglishFractionCalculationDateOrchestrator(_mediator.Object, logger.Object);
        _controller = new EnglishFractionCalculationDateController(orchestrator);
    }

    [Test]
    public async Task ThenReturnsOkWithTheLastCalculationDate()
    {
        const string empRef = "123/AB12345";
        var dateCalculated = new DateTime(2026, 4, 10);

        _mediator
            .Setup(x => x.Send(
                It.Is<GetLastEnglishFractionCalculationDateQuery>(query => query.EmpRef == empRef),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetLastEnglishFractionCalculationDateResponse
            {
                DateCalculated = dateCalculated
            });

        var result = await _controller.GetLastCalculationDate(empRef);

        result.Should().BeOfType<OkObjectResult>();
        var response = ((OkObjectResult)result).Value.Should()
            .BeOfType<LastEnglishFractionCalculationDateResult>().Subject;
        response.DateCalculated.Should().Be(dateCalculated);
    }

    [Test]
    public async Task ThenReturnsOkWithNullWhenNoCalculationDateExists()
    {
        const string empRef = "123/AB12345";

        _mediator
            .Setup(x => x.Send(
                It.Is<GetLastEnglishFractionCalculationDateQuery>(query => query.EmpRef == empRef),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetLastEnglishFractionCalculationDateResponse());

        var result = await _controller.GetLastCalculationDate(empRef);

        result.Should().BeOfType<OkObjectResult>();
        var response = ((OkObjectResult)result).Value.Should()
            .BeOfType<LastEnglishFractionCalculationDateResult>().Subject;
        response.DateCalculated.Should().BeNull();
    }

    [Test]
    public async Task ThenDecodesTheEmployerReference()
    {
        const string encodedEmpRef = "001%2FAC004317";
        const string decodedEmpRef = "001/AC004317";

        _mediator
            .Setup(x => x.Send(
                It.Is<GetLastEnglishFractionCalculationDateQuery>(query => query.EmpRef == decodedEmpRef),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetLastEnglishFractionCalculationDateResponse());

        await _controller.GetLastCalculationDate(encodedEmpRef);

        _mediator.Verify(x => x.Send(
                It.Is<GetLastEnglishFractionCalculationDateQuery>(query => query.EmpRef == decodedEmpRef),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task ThenReturnsBadRequestWhenTheEmployerReferenceIsInvalid()
    {
        var validationResult = new System.ComponentModel.DataAnnotations.ValidationResult(
            "Validation failed",
            ["EmpRef|EmpRef has not been supplied"]);

        _mediator
            .Setup(x => x.Send(
                It.Is<GetLastEnglishFractionCalculationDateQuery>(query => string.IsNullOrEmpty(query.EmpRef)),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException(validationResult, null, null));

        var result = await _controller.GetLastCalculationDate(string.Empty);

        result.Should().BeOfType<BadRequestObjectResult>();
        var response = ((BadRequestObjectResult)result).Value.Should()
            .BeOfType<Dictionary<string, string>>().Subject;
        response.Should().Contain("EmpRef", "EmpRef has not been supplied");
    }
}
