using MediatR;

namespace SFA.DAS.EmployerFinance.Queries.GetEmployerAccount
{
    public class GetEmployerAccountHashedQuery : IRequest<GetEmployerAccountResponse>
    {
        public string HashedAccountId { get; set; }
        public string UserId { get; set; }
    }
}
