namespace SFA.DAS.EmployerFinance.Queries.GetTransfersbyPeriodEnd;

public class GetTransfersByPeriodEndRequest : IRequest<GetTransfersByPeriodEndResponse>
{
    public long AccountId { get; set; }

    public string PeriodEnd { get; set; }
}
