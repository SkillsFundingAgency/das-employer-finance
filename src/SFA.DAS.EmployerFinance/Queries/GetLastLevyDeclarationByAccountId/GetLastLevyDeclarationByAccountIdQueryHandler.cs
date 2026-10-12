using SFA.DAS.EmployerFinance.Data.Contracts;

namespace SFA.DAS.EmployerFinance.Queries.GetLastLevyDeclarationByAccountId;

public class GetLastLevyDeclarationByAccountIdQueryHandler(IDasLevyRepository dasLevyRepository)
    : IRequestHandler<GetLastLevyDeclarationByAccountIdQuery, GetLastLevyDeclarationByAccountIdQueryResponse>
{
    public async Task<GetLastLevyDeclarationByAccountIdQueryResponse> Handle(GetLastLevyDeclarationByAccountIdQuery message, CancellationToken cancellationToken)
    {
        var result = await dasLevyRepository.GetLastPositiveNetDeclarationForAccount(message.AccountId);

        return new GetLastLevyDeclarationByAccountIdQueryResponse { Transaction = result };
    }
}