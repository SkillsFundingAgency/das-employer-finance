using SFA.DAS.EmployerFinance.Commands.SendTransferConnectionInvitation;
using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Models.Account;
using SFA.DAS.EmployerFinance.Models.TransferConnections;
using SFA.DAS.EmployerFinance.Models.Transfers;
using SFA.DAS.EmployerFinance.Models.UserProfile;
using SFA.DAS.EmployerFinance.TestCommon.Builders;
using SFA.DAS.Encoding;
using SFA.DAS.Testing;
using SFA.DAS.UnitOfWork.Context;

namespace SFA.DAS.EmployerFinance.UnitTests.Commands.SendTransferConnectionInvitationTests;

[TestFixture]
public class WhenIHandleSendTransferConnectionInvitationCommand
{
    [Test]
    public async Task Handle_SendTransferConnectionInvitationCommand_ThenShouldAddTransferConnectionInvitationToRepository()
    {
        var fixture = new WhenIHandleSendTransferConnectionInvitationCommandTestFixture();

        await fixture.Handle();
        
        fixture.TransferConnectionInvitationRepository.Verify(repository => repository.Add(It.IsAny<TransferConnectionInvitation>()), Times.Once);
    }
}

public class WhenIHandleSendTransferConnectionInvitationCommandTestFixture 
{
    private SendTransferConnectionInvitationCommandHandler Handler { get; }
    private SendTransferConnectionInvitationCommand Command { get; }
    private Mock<IEmployerAccountRepository> EmployerAccountRepository { get; }
    public Mock<ITransferConnectionInvitationRepository> TransferConnectionInvitationRepository { get; }
    private Mock<ITransferRepository> TransferRepository { get; }
    private Mock<IUserAccountRepository> UserRepository { get; }
    private Mock<IEncodingService> EncodingService { get; }
    private EmployerFinanceConfiguration EmployerFinanceConfiguration { get; }
    private Account ReceiverAccount { get; set; }
    public long? Result { get; set; }
    private Account SenderAccount { get; set; }
    private User SenderUser { get; set; }
    public IUnitOfWorkContext UnitOfWorkContext { get; }

    public WhenIHandleSendTransferConnectionInvitationCommandTestFixture()
    {
        EmployerAccountRepository = new Mock<IEmployerAccountRepository>();
        TransferConnectionInvitationRepository = new Mock<ITransferConnectionInvitationRepository>();
        TransferRepository = new Mock<ITransferRepository>();
        UserRepository = new Mock<IUserAccountRepository>();
        EncodingService = new Mock<IEncodingService>();
        EmployerFinanceConfiguration = new EmployerFinanceConfiguration
        {
            TransferAllowancePercentage = 1
        };

        SetSenderAccount()
            .SetReceiverAccount()
            .SetSenderUser()
            .SetSenderAccountTransferAllowance(1);

        Handler = new SendTransferConnectionInvitationCommandHandler
        (
            EmployerAccountRepository.Object,
            TransferConnectionInvitationRepository.Object,
            TransferRepository.Object,
            UserRepository.Object,
            EmployerFinanceConfiguration,
            EncodingService.Object,
            Mock.Of<ILogger<SendTransferConnectionInvitationCommandHandler>>()
        );

        Command = new SendTransferConnectionInvitationCommand
        {
            AccountId = SenderAccount.Id,
            UserRef = SenderUser.Ref,
            ReceiverAccountPublicHashedId = ReceiverAccount.PublicHashedId
        };

        UnitOfWorkContext = new UnitOfWorkContext();
    }

    private WhenIHandleSendTransferConnectionInvitationCommandTestFixture AddAccount(Account account)
    {
        EmployerAccountRepository.Setup(r => r.Get(account.Id)).ReturnsAsync(account);
        EncodingService.Setup(h => h.Decode(account.PublicHashedId, EncodingType.PublicAccountId)).Returns(account.Id);

        return this;
    }

    public WhenIHandleSendTransferConnectionInvitationCommandTestFixture AddInvitationFromSenderToReceiver(TransferConnectionInvitationStatus status)
    {
        SenderAccount.SentTransferConnectionInvitations.Add(
            new TransferConnectionInvitationBuilder()
                .WithReceiverAccount(ReceiverAccount)
                .WithStatus(status)
                .Build());

        return this;
    }

    public async Task Handle()
    {
        Result = await Handler.Handle(Command,CancellationToken.None);
    }

    private WhenIHandleSendTransferConnectionInvitationCommandTestFixture SetReceiverAccount()
    {
        const string hashedAccountId = "ABC123";
        ReceiverAccount = new Account
        {
            Id = 222222,
            Name = "Receiver",
            HashedId = hashedAccountId
        };

        return AddAccount(ReceiverAccount);
    }

    private WhenIHandleSendTransferConnectionInvitationCommandTestFixture SetSenderAccount()
    {
        const string hashedAccountId = "ABC123";
        SenderAccount = new Account
        {
            Id = 333333,
            Name = "Sender",
            HashedId = hashedAccountId
        };

        return AddAccount(SenderAccount);
    }

    private void SetSenderAccountTransferAllowance(decimal remainingTransferAllowance)
    {
        var transferAllowance = new TransferAllowance { RemainingTransferAllowance = remainingTransferAllowance };

        TransferRepository.Setup(s => s.GetTransferAllowance(SenderAccount.Id, It.IsAny<decimal>())).ReturnsAsync(transferAllowance);
    }

    private WhenIHandleSendTransferConnectionInvitationCommandTestFixture SetSenderUser()
    {
        SenderUser = new User
        {
            Ref = Guid.NewGuid(),
            Id = 123456,
            FirstName = "John",
            LastName = "Doe"
        };

        UserRepository
            .Setup(r => r.Get(SenderUser.Ref))
            .ReturnsAsync(SenderUser);

        return this;
    }
}