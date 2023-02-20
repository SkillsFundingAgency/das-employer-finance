using SFA.DAS.EmployerFinance.Commands.RemoveAccountLegalEntity;
using SFA.DAS.EmployerFinance.Data.Contracts;

namespace SFA.DAS.EmployerFinance.UnitTests.Commands.RemoveAccountLegalEntityTests;

public class WhenIHandleTheCommand
{
    private RemoveAccountLegalEntityCommandHandler _handler;
    private Mock<IAccountLegalEntityRepository> _accountLegalEntityRepository;

    [SetUp]
    public void Arrange()
    {
        _accountLegalEntityRepository = new Mock<IAccountLegalEntityRepository>();

        _handler = new RemoveAccountLegalEntityCommandHandler(_accountLegalEntityRepository.Object, Mock.Of<ILogger<RemoveAccountLegalEntityCommandHandler>>());
    }

    [Test]
    public async Task ThenTheAccountLegalEntityIsRemoved()
    {
        var id = 234985;

        await _handler.Handle(new RemoveAccountLegalEntityCommand(id), CancellationToken.None);

        _accountLegalEntityRepository.Verify(x => x.RemoveAccountLegalEntity(id));
    }
}