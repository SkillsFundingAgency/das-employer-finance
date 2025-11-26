using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Queries.GetAccount;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerFinance.Queries.GetAccountPaymentIds;

public class GetAccountPaymentIdsQueryHandler : IRequestHandler<GetAccountPaymentIdsRequest, GetAccountPaymentIdsResponse>
{
    private readonly IDasLevyRepository _dasLevyRepository;

    public GetAccountPaymentIdsQueryHandler(IDasLevyRepository dasLevyRepository)
    {
        _dasLevyRepository = dasLevyRepository;
    }

    public async Task<GetAccountPaymentIdsResponse> Handle(GetAccountPaymentIdsRequest message, CancellationToken cancellationToken)
    {
        var response = new GetAccountPaymentIdsResponse();
        response.PaymentIds = (await _dasLevyRepository.GetAccountPaymentIdsLinq(message.AccountId)).ToList();
        return response;
    }
}
