namespace SFA.DAS.EmployerFinance.Queries.GetTransferAllowance;

public class GetTransferAllowanceQuery : IRequest<GetTransferAllowanceResponse>
{
    public long AccountId { get; set; }
}