using AutoMapper;
using SFA.DAS.EmployerFinance.Api.Controllers;
using SFA.DAS.EmployerFinance.Api.Mappings;
using SFA.DAS.EmployerFinance.Api.Orchestrators;
using SFA.DAS.EmployerFinance.Api.Types;
using SFA.DAS.EmployerFinance.Models.Levy;
using SFA.DAS.EmployerFinance.Queries.GetLastLevyDeclaration;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerFinance.Api.UnitTests.Controllers.PayeSchemesControllerTests;

public class WhenIGetLastSubmissionDate
{
    private PayeSchemesController _payeSchemesController = null!;
    private Mock<IMediator> _mediator = null!;
    private Mock<ILogger<FinanceOrchestrator>> _logger = null!;
    private Mapper _mapper = null!;
    private Mock<IEncodingService> _encodingService = null!;

    [SetUp]
    public void Arrange()
    {
        _mediator = new Mock<IMediator>();
        _logger = new Mock<ILogger<FinanceOrchestrator>>();
        _encodingService = new Mock<IEncodingService>();

        _mapper = new Mapper(new MapperConfiguration(c =>
        {
            c.AddProfile<AccountMappings>();
        }));

        var orchestrator = new FinanceOrchestrator(
            _mediator.Object,
            _logger.Object,
            _mapper,
            _encodingService.Object);

        _payeSchemesController = new PayeSchemesController(orchestrator);
    }

    [Test]
    public async Task ThenReturnOkWithLastSubmissionDateWhenDeclarationExists()
    {
        const string empRef = "123/AB12345";
        var expectedDate = new DateTime(2026, 4, 1);

        _mediator
            .Setup(x => x.Send(
                It.Is<GetLastLevyDeclarationQuery>(q => q.EmpRef == empRef),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetLastLevyDeclarationResponse
            {
                Transaction = new DasDeclaration { SubmissionDate = expectedDate }
            });

        var result = await _payeSchemesController.GetLastSubmissionDate(empRef) as OkObjectResult;

        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(200);

        var responseObject = result.Value as PayeSchemeLastSubmissionDate;
        responseObject.Should().NotBeNull();
        responseObject!.EmpRef.Should().Be(empRef);
        responseObject.LastSubmissionDate.Should().Be(expectedDate);
    }

    [Test]
    public async Task ThenReturnOkWithNullLastSubmissionDateWhenNoDeclarationExists()
    {
        const string empRef = "123/AB12345";

        _mediator
            .Setup(x => x.Send(
                It.Is<GetLastLevyDeclarationQuery>(q => q.EmpRef == empRef),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetLastLevyDeclarationResponse { Transaction = null });

        var result = await _payeSchemesController.GetLastSubmissionDate(empRef) as OkObjectResult;

        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(200);

        var responseObject = result.Value as PayeSchemeLastSubmissionDate;
        responseObject.Should().NotBeNull();
        responseObject!.EmpRef.Should().Be(empRef);
        responseObject.LastSubmissionDate.Should().BeNull();
    }

    [Test]
    public async Task ThenReturnOkWithNullLastSubmissionDateWhenSubmissionDateIsMinValue()
    {
        const string empRef = "123/AB12345";

        _mediator
            .Setup(x => x.Send(
                It.Is<GetLastLevyDeclarationQuery>(q => q.EmpRef == empRef),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetLastLevyDeclarationResponse
            {
                Transaction = new DasDeclaration { SubmissionDate = DateTime.MinValue }
            });

        var result = await _payeSchemesController.GetLastSubmissionDate(empRef) as OkObjectResult;

        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(200);

        var responseObject = result.Value as PayeSchemeLastSubmissionDate;
        responseObject.Should().NotBeNull();
        responseObject!.LastSubmissionDate.Should().BeNull();
    }

    [Test]
    public async Task ThenReturnBadRequestWhenEmpRefIsEmpty()
    {
        _mediator
            .Setup(x => x.Send(
                It.Is<GetLastLevyDeclarationQuery>(q => string.IsNullOrEmpty(q.EmpRef)),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new System.ComponentModel.DataAnnotations.ValidationException("EmpRef is required"));

        var result = await _payeSchemesController.GetLastSubmissionDate(string.Empty);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Test]
    public async Task ThenAcceptEmpRefWithSlash()
    {
        const string empRef = "001/AC004317";
        var expectedDate = new DateTime(2025, 6, 15);

        _mediator
            .Setup(x => x.Send(
                It.Is<GetLastLevyDeclarationQuery>(q => q.EmpRef == empRef),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetLastLevyDeclarationResponse
            {
                Transaction = new DasDeclaration { SubmissionDate = expectedDate }
            });

        var result = await _payeSchemesController.GetLastSubmissionDate(empRef) as OkObjectResult;

        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(200);

        var responseObject = result.Value as PayeSchemeLastSubmissionDate;
        responseObject.Should().NotBeNull();
        responseObject!.EmpRef.Should().Be(empRef);
        responseObject.LastSubmissionDate.Should().Be(expectedDate);
    }

    [Test]
    public async Task ThenAcceptUrlEncodedEmpRefQueryValue()
    {
        const string empRef = "001%2FAC004317";
        const string decodedEmpRef = "001/AC004317";
        var expectedDate = new DateTime(2025, 6, 15);

        _mediator
            .Setup(x => x.Send(
                It.Is<GetLastLevyDeclarationQuery>(q => q.EmpRef == decodedEmpRef),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetLastLevyDeclarationResponse
            {
                Transaction = new DasDeclaration { SubmissionDate = expectedDate }
            });

        var result = await _payeSchemesController.GetLastSubmissionDate(empRef) as OkObjectResult;

        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(200);

        var responseObject = result.Value as PayeSchemeLastSubmissionDate;
        responseObject.Should().NotBeNull();
        responseObject!.EmpRef.Should().Be(decodedEmpRef);
    }
}
