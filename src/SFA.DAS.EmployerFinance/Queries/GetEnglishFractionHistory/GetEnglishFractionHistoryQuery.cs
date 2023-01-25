using MediatR;

namespace SFA.DAS.EmployerFinance.Queries.GetEnglishFrationHistory
{
    public class GetEnglishFractionHistoryQuery : IRequest<GetEnglishFractionHistoryResposne>
    {
        public string HashedAccountId { get; set; }
        public string EmpRef { get; set; }
    }
}