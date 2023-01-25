using MediatR;
using System.Collections.Generic;

namespace SFA.DAS.EmployerFinance.Queries.GetAccountBalances
{
    public class GetAccountBalancesRequest : IRequest<GetAccountBalancesResponse>
    {
        public List<long> AccountIds { get; set; }
    }
}
