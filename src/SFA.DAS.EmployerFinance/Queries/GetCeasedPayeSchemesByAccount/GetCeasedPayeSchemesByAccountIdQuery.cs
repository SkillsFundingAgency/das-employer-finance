namespace SFA.DAS.EmployerFinance.Queries.GetCeasedPayeSchemesByAccount;

public sealed record GetCeasedPayeSchemesByAccountIdQuery(long AccountId)
    : IRequest<GetCeasedPayeSchemesByAccountIdResponse>;