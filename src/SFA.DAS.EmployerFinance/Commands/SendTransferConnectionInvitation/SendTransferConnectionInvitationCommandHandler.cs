using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerFinance.Commands.SendTransferConnectionInvitation;

public class SendTransferConnectionInvitationCommandHandler : IRequestHandler<SendTransferConnectionInvitationCommand, int>
{
    private readonly IEmployerAccountRepository _employerAccountRepository;
    private readonly ITransferConnectionInvitationRepository _transferConnectionInvitationRepository;
    private readonly ITransferRepository _transferRepository;
    private readonly IUserAccountRepository _userRepository;
    private readonly EmployerFinanceConfiguration _configuration;
    private readonly IEncodingService _encodingService;
    private readonly ILogger<SendTransferConnectionInvitationCommandHandler> _logger;

    public SendTransferConnectionInvitationCommandHandler(
        IEmployerAccountRepository employerAccountRepository,            
        ITransferConnectionInvitationRepository transferConnectionInvitationRepository,
        ITransferRepository transferRepository,
        IUserAccountRepository userRepository,
        EmployerFinanceConfiguration configuration,
        IEncodingService encodingService,
        ILogger<SendTransferConnectionInvitationCommandHandler> logger)
    {
        _employerAccountRepository = employerAccountRepository;
        _transferConnectionInvitationRepository = transferConnectionInvitationRepository;
        _transferRepository = transferRepository;
        _userRepository = userRepository;
        _configuration = configuration;
        _encodingService = encodingService;
        _logger = logger;
    }

    public async Task<int> Handle(SendTransferConnectionInvitationCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting {TypeName}.", nameof(SendTransferConnectionInvitationCommandHandler));
        
        var receiverAccountId = _encodingService.Decode(request.ReceiverAccountPublicHashedId, EncodingType.PublicAccountId);
        var senderAccount = await _employerAccountRepository.Get(request.AccountId);
        var receiverAccount = await _employerAccountRepository.Get(receiverAccountId);
        var senderUser = await _userRepository.Get(request.UserRef);

        receiverAccount.HashedId = request.ReceiverAccountPublicHashedId;
        
        _logger.LogInformation("Retrieving transfer allowance for {AccountId}.", request.AccountId);
        
        var senderAccountTransferAllowance = await _transferRepository.GetTransferAllowance(request.AccountId, _configuration.TransferAllowancePercentage);
        
        _logger.LogInformation("Sending transfer connection invitation for {AccountId}.", request.AccountId);
        
        var transferConnectionInvitation = senderAccount.SendTransferConnectionInvitation(receiverAccount, senderUser, senderAccountTransferAllowance.RemainingTransferAllowance ?? 0);

        _logger.LogInformation("Adding invitation to repository for for {AccountId}.", request.AccountId);
        
        await _transferConnectionInvitationRepository.Add(transferConnectionInvitation);
        
        _logger.LogInformation("Completed {TypeName}.", nameof(SendTransferConnectionInvitationCommandHandler));

        return transferConnectionInvitation.Id;
    }
}