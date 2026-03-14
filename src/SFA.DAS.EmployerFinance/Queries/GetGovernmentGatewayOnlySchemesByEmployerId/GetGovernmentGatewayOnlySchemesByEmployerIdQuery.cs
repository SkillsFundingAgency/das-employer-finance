namespace SFA.DAS.EmployerFinance.Queries.GetGovernmentGatewayOnlySchemesByEmployerId;

public class GetGovernmentGatewayOnlySchemesByEmployerIdQuery : IRequest<GetGovernmentGatewayOnlySchemesByEmployerIdResponse>
{
    public long AccountId { get; set; }
}
