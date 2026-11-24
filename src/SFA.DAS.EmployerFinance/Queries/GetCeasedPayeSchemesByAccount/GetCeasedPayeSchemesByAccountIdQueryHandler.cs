using SFA.DAS.EmployerFinance.Data.Contracts;

namespace SFA.DAS.EmployerFinance.Queries.GetCeasedPayeSchemesByAccount;

public class GetCeasedPayeSchemesByAccountIdQueryHandler(IDasLevyRepository dasLevyRepository) : IRequestHandler<GetCeasedPayeSchemesByAccountIdQuery,
    GetCeasedPayeSchemesByAccountIdResponse>
{
    public async Task<GetCeasedPayeSchemesByAccountIdResponse> Handle(GetCeasedPayeSchemesByAccountIdQuery request, CancellationToken cancellationToken)
    {
        var payeSchemes = await dasLevyRepository.GetAccountCeasedPayeSchemes(request.AccountId);

        return new GetCeasedPayeSchemesByAccountIdResponse
        {
            PayeSchemes = payeSchemes
        };
    }
}