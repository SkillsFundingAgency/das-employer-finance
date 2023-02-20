using SFA.DAS.EmployerFinance.Commands.RemoveAccountPaye;
using SFA.DAS.EmployerFinance.Data.Contracts;

namespace SFA.DAS.EmployerFinance.UnitTests.Commands.RemoveAccountPayeTests;

public class WhenIHandleTheCommand
{
    private RemoveAccountPayeCommandHandler _handler;
    private Mock<IPayeRepository> _payeRepository;

    [SetUp]
    public void Arrange()
    {
        _payeRepository = new Mock<IPayeRepository>();
        _handler = new RemoveAccountPayeCommandHandler(_payeRepository.Object, Mock.Of<ILogger<RemoveAccountPayeCommandHandler>>());
    }

    [Test]
    public async Task ThenThePayeSchemeIsRemoved()
    {
        var accountId = 123443;
        var payeRef = "ABC/12343534";

        await _handler.Handle(new RemoveAccountPayeCommand(accountId, payeRef), CancellationToken.None);

        _payeRepository.Verify(x => x.RemovePayeScheme(accountId, payeRef));
    }
}