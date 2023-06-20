using SFA.DAS.EmployerFinance.Commands.CreateAccount;
using SFA.DAS.EmployerFinance.Data.Contracts;

namespace SFA.DAS.EmployerFinance.UnitTests.Commands.CreateAccountTests;

public class WhenIHandleTheCommand
{
    private CreateAccountCommandHandler _handler;
    private Mock<IAccountRepository> _accountRepository;
        
    [SetUp]
    public void Arrange()
    {
        _accountRepository = new Mock<IAccountRepository>();
        _handler = new CreateAccountCommandHandler(_accountRepository.Object, Mock.Of<ILogger<CreateAccountCommandHandler>>());
    }

    [Test]
    public async Task ThenTheAccountIsCreated()
    {
        var accountId = 123443;
        var name = "Account Name";

        await _handler.Handle(new CreateAccountCommand(accountId, name), CancellationToken.None);

        _accountRepository.Verify(x => x.CreateAccount(accountId, name));
    }
}