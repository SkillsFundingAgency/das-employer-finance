namespace SFA.DAS.EmployerFinance.Queries.GetPayeSchemesByEmployerId;

public class GetPayeSchemesByEmployerIdQuery : IRequest<GetPayeSchemesByEmployerIdResponse>
{
    public long AccountId { get; set; }
    public string? Source { get; set; }
}
