﻿using SFA.DAS.EmployerFinance.Commands.CreateAccountLegalEntity;
using SFA.DAS.EmployerFinance.Data.Contracts;

namespace SFA.DAS.EmployerFinance.UnitTests.Commands.CreateAccountLegalEntityTests;

public class WhenIHandleTheCommand
{
    private CreateAccountLegalEntityCommandHandler _handler;
    private Mock<IAccountLegalEntityRepository> _accountLegalEntityRepository;

    [SetUp]
    public void Arrange()
    {
        _accountLegalEntityRepository = new Mock<IAccountLegalEntityRepository>();

        _handler = new CreateAccountLegalEntityCommandHandler(_accountLegalEntityRepository.Object, Mock.Of<ILogger<CreateAccountLegalEntityCommandHandler>>());
    }

    [Test]
    public async Task ThenTheAccountLegalEntityIsCreated()
    {
        var id = 337782;
        var pendingAgreementId = 56832;
        var signedAgreementId = 338921;
        var signedAgreementVersion = 3;
        var accountId = 10862;
        var legalEntityId = 44893;

        await _handler.Handle(new CreateAccountLegalEntityCommand(id, pendingAgreementId, signedAgreementId, signedAgreementVersion, accountId, legalEntityId),CancellationToken.None);

        _accountLegalEntityRepository.Verify(x => x.CreateAccountLegalEntity(id, pendingAgreementId, signedAgreementId, signedAgreementVersion, accountId, legalEntityId));
    }
}