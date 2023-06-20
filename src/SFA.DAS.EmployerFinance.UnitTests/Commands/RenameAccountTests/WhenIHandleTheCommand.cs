using SFA.DAS.EmployerFinance.Commands.RenameAccount;
using SFA.DAS.EmployerFinance.Data.Contracts;

namespace SFA.DAS.EmployerFinance.UnitTests.Commands.RenameAccountTests;

public class WhenIHandleTheCommand
{
    private RenameAccountCommandHandler _handler;
    private Mock<IAccountRepository> _accountRepository;
        
    [SetUp]
    public void Arrange()
    {
        _accountRepository = new Mock<IAccountRepository>();

        _handler = new RenameAccountCommandHandler(_accountRepository.Object, Mock.Of<ILogger<RenameAccountCommandHandler>>());
    }

    [Test]
    public async Task ThenTheAccountIsRenamed()
    {
        var accountId = 123443;
        var name = "Account Name";

        await _handler.Handle(new RenameAccountCommand(accountId, name), CancellationToken.None);

        _accountRepository.Verify(x => x.RenameAccount(accountId, name));
    }
}