using AutoMapper;
using SFA.DAS.EmployerFinance.Api.Controllers;
using SFA.DAS.EmployerFinance.Api.Orchestrators;
using SFA.DAS.EmployerFinance.Queries.GetCeasedPayeSchemesByAccount;

namespace SFA.DAS.EmployerFinance.Api.UnitTests.Controllers.FinanceLevyDeclarationsControllerTests;

[TestFixture]
internal class WhenIGetCeasedPayeSchemes
{
    private const long AccountId = 12345;

    private FinanceLevyDeclarationsController _controller;
    private Mock<IMediator> _mediator;

    [SetUp]
    public void Arrange()
    {
        _mediator = new Mock<IMediator>();

        var orchestrator = new LevyDeclarationOrchestrator(_mediator.Object, Mock.Of<IMapper>(), Mock.Of<ILogger<LevyDeclarationOrchestrator>>());

        _controller = new FinanceLevyDeclarationsController(orchestrator);
    }

    [Test]
    public async Task Then_Returns_The_Ceased_Paye_Schemes_For_The_Account()
    {
        _mediator
            .Setup(x => x.Send(It.Is<GetCeasedPayeSchemesByAccountIdQuery>(q => q.AccountId == AccountId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetCeasedPayeSchemesByAccountIdResponse { PayeSchemes = ["123/AB12345", "456/CD67890"] });

        var result = await _controller.GetCeasedPayeSchemes(AccountId);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(new List<string> { "123/AB12345", "456/CD67890" });
    }

    [Test]
    public async Task Then_Returns_Ok_With_An_Empty_List_When_There_Are_No_Ceased_Paye_Schemes()
    {
        _mediator
            .Setup(x => x.Send(It.IsAny<GetCeasedPayeSchemesByAccountIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetCeasedPayeSchemesByAccountIdResponse { PayeSchemes = [] });

        var result = await _controller.GetCeasedPayeSchemes(AccountId);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeAssignableTo<IEnumerable<string>>().Which.Should().BeEmpty();
    }
}
