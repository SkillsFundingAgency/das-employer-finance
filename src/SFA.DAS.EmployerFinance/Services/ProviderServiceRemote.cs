﻿using SFA.DAS.EmployerFinance.Infrastructure.OuterApiRequests.Providers;
using SFA.DAS.EmployerFinance.Infrastructure.OuterApiResponses.Providers;
using SFA.DAS.EmployerFinance.Interfaces.OuterApi;
using SFA.DAS.EmployerFinance.Services.Contracts;

namespace SFA.DAS.EmployerFinance.Services;

public class ProviderServiceRemote : IProviderService
{
    private readonly IProviderService _providerService;
    private readonly IOuterApiClient _outerApiClient;
    private readonly ILogger<ProviderServiceRemote> _logger;

    public ProviderServiceRemote(IProviderService providerService, IOuterApiClient apiClient, ILogger<ProviderServiceRemote> logger)
    {
        _providerService = providerService;
        _outerApiClient = apiClient;
        _logger = logger;
    }
     
    public async Task<Models.ApprenticeshipProvider.Provider> Get(long ukPrn)
    {
        try
        {
            var provider = await _outerApiClient.Get<GetProviderResponse>(new GetProviderRequest(ukPrn));
            if(provider != null)
            {
                return MapFrom(provider);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, $"Unable to get provider details with UKPRN {ukPrn} from apprenticeship API.");
        }
            
        return await _providerService.Get(ukPrn);
    }

    private static Models.ApprenticeshipProvider.Provider MapFrom(GetProviderResponse provider)
    {
        return new Models.ApprenticeshipProvider.Provider()
        {
            Ukprn = provider.Ukprn,
            Name = provider.Name,
            Email = provider.Email,
            Phone = provider.Phone
        };
    }
}