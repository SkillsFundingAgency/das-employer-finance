using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Data.Contracts;

namespace SFA.DAS.EmployerFinance.Queries.GetTransferConnectionInvitationAuthorization;

public class GetTransferConnectionInvitationAuthorizationQueryHandler : IRequestHandler<GetTransferConnectionInvitationAuthorizationQuery, GetTransferConnectionInvitationAuthorizationResponse>
{
    private readonly ITransferRepository _transferRepository;
    private readonly EmployerFinanceConfiguration _configuration;
    //private readonly IAuthorizationService _authorizationService;

    public GetTransferConnectionInvitationAuthorizationQueryHandler(
        ITransferRepository transferRepository,
        EmployerFinanceConfiguration configuration
        //, IAuthorizationService authorizationService //TODO 
        )
    {
        _transferRepository = transferRepository;
        _configuration = configuration;
        //_authorizationService = authorizationService;
    }

    public async Task<GetTransferConnectionInvitationAuthorizationResponse> Handle(GetTransferConnectionInvitationAuthorizationQuery message,CancellationToken cancellationToken)
    {
        //var authorizationResult = new AuthorizationResult();// await _authorizationService.GetAuthorizationResultAsync("EmployerFeature.TransferConnectionRequests");
        var transferAllowance = await _transferRepository.GetTransferAllowance(message.AccountId, _configuration.TransferAllowancePercentage);

        var isValidSender = transferAllowance.RemainingTransferAllowance >= Constants.TransferConnectionInvitations.SenderMinTransferAllowance;

        return new GetTransferConnectionInvitationAuthorizationResponse
        {
            //AuthorizationResult = authorizationResult,
            IsValidSender = isValidSender,
            TransferAllowancePercentage = _configuration.TransferAllowancePercentage
        };
    }
}