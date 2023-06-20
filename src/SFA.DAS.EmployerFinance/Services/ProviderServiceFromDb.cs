using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Services.Contracts;

namespace SFA.DAS.EmployerFinance.Services;

public class ProviderServiceFromDb : IProviderService
{
    private readonly IDasLevyRepository _dasLevyRepository;
    private readonly ILogger<ProviderServiceFromDb> _logger;

    public ProviderServiceFromDb(IDasLevyRepository dasLevyRepository, ILogger<ProviderServiceFromDb> logger)
    {
        _dasLevyRepository = dasLevyRepository;
        _logger = logger;
    }

    public async Task<Models.ApprenticeshipProvider.Provider> Get(long ukPrn)
    {
        _logger.LogInformation($"Getting provider name from previous payments for Ukprn: {ukPrn}");
        var providerName = await _dasLevyRepository.FindHistoricalProviderName(ukPrn);

        if(providerName == null)
        {
            _logger.LogWarning($"No provider name found for Ukprn:{ukPrn} in previous payments");
        }

        _logger.LogInformation($"Provider Name {providerName} found for Ukprn: {ukPrn}.");

        return new Models.ApprenticeshipProvider.Provider
        {
            Name = providerName,
            Ukprn = ukPrn,
            IsHistoricProviderName = true
        };
    }
}