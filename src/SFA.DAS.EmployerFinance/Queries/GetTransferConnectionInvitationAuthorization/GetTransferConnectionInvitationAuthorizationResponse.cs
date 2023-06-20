

namespace SFA.DAS.EmployerFinance.Queries.GetTransferConnectionInvitationAuthorization;

public class GetTransferConnectionInvitationAuthorizationResponse
{
    public bool AuthorizationResult { get; set; }
    public bool AgreementSigned { get; set; }
    public bool IsValidSender { get; set; }
    public decimal TransferAllowancePercentage { get; set; }
}