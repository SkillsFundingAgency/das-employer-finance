using SFA.DAS.EmployerFinance.Models.Transfers;

namespace SFA.DAS.EmployerFinance.Queries.GetTransfersbyPeriodEnd;

public class GetTransfersByPeriodEndResponse
{
    public List<AccountTransfer> AccountTransfers { get; set; }
}
