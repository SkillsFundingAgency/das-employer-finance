namespace SFA.DAS.EmployerFinance.Queries.GetAccountProjectionSummary
{
    public class GetAccountProjectionSummaryQuery : IRequest<GetAccountProjectionSummaryResult>
    {
        public long AccountId { get; set; }
    }
}
