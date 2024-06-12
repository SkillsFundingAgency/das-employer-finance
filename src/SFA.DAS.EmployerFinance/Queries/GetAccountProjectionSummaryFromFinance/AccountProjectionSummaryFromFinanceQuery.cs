namespace SFA.DAS.EmployerFinance.Queries.GetAccountProjectionSummaryFromFinance
{
    public class AccountProjectionSummaryFromFinanceQuery : IRequest<AccountProjectionSummaryFromFinanceResult>
    {
        public long AccountId { get; set; }
    }
}
