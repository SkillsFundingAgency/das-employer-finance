using SFA.DAS.EmployerFinance.Api.Orchestrators;
using SFA.DAS.EmployerFinance.Api.Types;
using SFA.DAS.EmployerFinance.Queries.GetLastEnglishFractionCalculationDate;

namespace SFA.DAS.EmployerFinance.Api.UnitTests.Orchestrators;

public class EnglishFractionCalculationDateOrchestratorTests
{
    [Test]
    public async Task Then_Returns_DateCalculated_From_Query_Response()
    {
        var empRef = "123/AB12345";
        var dateCalculated = new DateTime(2026, 4, 10);
        var mediator = new Mock<IMediator>();
        var logger = new Mock<ILogger<EnglishFractionCalculationDateOrchestrator>>();

        mediator.Setup(x => x.Send(
                It.Is<GetLastEnglishFractionCalculationDateQuery>(q => q.EmpRef == empRef),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetLastEnglishFractionCalculationDateResponse
            {
                DateCalculated = dateCalculated
            });

        var sut = new EnglishFractionCalculationDateOrchestrator(mediator.Object, logger.Object);

        var result = await sut.GetLastCalculationDate(empRef);

        result.Should().NotBeNull();
        result.DateCalculated.Should().Be(dateCalculated);
    }

    [Test]
    public async Task Then_Returns_Null_DateCalculated_When_No_Stored_Fractions()
    {
        var empRef = "123/AB12345";
        var mediator = new Mock<IMediator>();
        var logger = new Mock<ILogger<EnglishFractionCalculationDateOrchestrator>>();

        mediator.Setup(x => x.Send(
                It.Is<GetLastEnglishFractionCalculationDateQuery>(q => q.EmpRef == empRef),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetLastEnglishFractionCalculationDateResponse
            {
                DateCalculated = null
            });

        var sut = new EnglishFractionCalculationDateOrchestrator(mediator.Object, logger.Object);

        var result = await sut.GetLastCalculationDate(empRef);

        result.Should().NotBeNull();
        result.DateCalculated.Should().BeNull();
    }
}
