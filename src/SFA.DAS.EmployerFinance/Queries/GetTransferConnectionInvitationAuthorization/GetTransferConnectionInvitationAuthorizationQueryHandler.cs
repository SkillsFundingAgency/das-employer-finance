using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Infrastructure.OuterApiRequests.Accounts;
using SFA.DAS.EmployerFinance.Infrastructure.OuterApiResponses.Accounts;
using SFA.DAS.EmployerFinance.Interfaces.OuterApi;

namespace SFA.DAS.EmployerFinance.Queries.GetTransferConnectionInvitationAuthorization;

public class GetTransferConnectionInvitationAuthorizationQueryHandler : IRequestHandler<GetTransferConnectionInvitationAuthorizationQuery, GetTransferConnectionInvitationAuthorizationResponse>
{
    private readonly ITransferRepository _transferRepository;
    private readonly EmployerFinanceConfiguration _configuration;
    private readonly IOuterApiClient _outerApiClient;

    public GetTransferConnectionInvitationAuthorizationQueryHandler(
        ITransferRepository transferRepository,
        EmployerFinanceConfiguration configuration,
        IOuterApiClient outerApiClient
        )
    {
        _transferRepository = transferRepository;
        _configuration = configuration;
        _outerApiClient = outerApiClient;
    }

    public async Task<GetTransferConnectionInvitationAuthorizationResponse> Handle(GetTransferConnectionInvitationAuthorizationQuery message,CancellationToken cancellationToken)
    {
        var transferAllowanceTask = _transferRepository.GetTransferAllowance(message.AccountId, _configuration.TransferAllowancePercentage);
        var agreementTask = _outerApiClient
            .Get<GetMinimumSignedAgreementVersionResponse>(
                new GetMinimumSignedAgreementVersionRequest(message.AccountId));

        await Task.WhenAll(transferAllowanceTask, agreementTask);
        
        var isValidSender = transferAllowanceTask.Result.RemainingTransferAllowance >= Constants.TransferConnectionInvitations.SenderMinTransferAllowance;

        return new GetTransferConnectionInvitationAuthorizationResponse
        {
            IsValidSender = isValidSender,
            TransferAllowancePercentage = _configuration.TransferAllowancePercentage,
            AuthorizationResult = agreementTask.Result.MinimumSignedAgreementVersion >= Constants.TransferConnectionInvitations.MinimumSignedAgreementVersion
        };
    }
}