using SFA.DAS.EmployerFinance.Queries.GetLastLevyDeclaration;

namespace SFA.DAS.EmployerFinance.Queries.GetLastLevyDeclarationByAccountId;

public sealed record GetLastLevyDeclarationByAccountIdQuery(long AccountId) : IRequest<GetLastLevyDeclarationByAccountIdQueryResponse>;