using MediatR;

namespace SFA.DAS.EmployerFinance.Queries.GetAccountTransactionSummary
{
    public class GetAccountTransactionSummaryRequest : IRequest<GetAccountTransactionSummaryResponse>
    {
        public string HashedAccountId { get; set; }
    }
}
