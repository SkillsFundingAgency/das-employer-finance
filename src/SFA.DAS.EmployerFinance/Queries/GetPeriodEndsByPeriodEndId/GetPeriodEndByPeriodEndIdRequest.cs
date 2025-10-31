namespace SFA.DAS.EmployerFinance.Queries.GetPeriodEnds;

public class GetPeriodEndByPeriodEndIdRequest : IRequest<GetPeriodEndByPeriodEndIdResponse>
{
    public string PeriodEndId { get; set; }
}