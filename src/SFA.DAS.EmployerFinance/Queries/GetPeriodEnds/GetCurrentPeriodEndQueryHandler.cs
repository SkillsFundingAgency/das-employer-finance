﻿using SFA.DAS.EmployerFinance.Data.Contracts;

namespace SFA.DAS.EmployerFinance.Queries.GetPeriodEnds;

public class GetPeriodEndQueryHandler : IRequestHandler<GetPeriodEndsRequest, GetPeriodEndsResponse>
{
    private readonly IDasLevyRepository _dasLevyRepository;

    public GetPeriodEndQueryHandler(IDasLevyRepository dasLevyRepository)
    {
        _dasLevyRepository = dasLevyRepository;
    }

    public async Task<GetPeriodEndsResponse> Handle(GetPeriodEndsRequest message,CancellationToken cancellationToken)
    {
        var response = new GetPeriodEndsResponse();

        var result = await _dasLevyRepository.GetAllPeriodEnds();
        response.CurrentPeriodEnds = result.ToList();

        return response;
    }
}