using Microsoft.AspNetCore.Http;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Exceptions;
using SFA.DAS.EmployerFinance.Interfaces;
using SFA.DAS.EmployerFinance.Services;
using SFA.DAS.EmployerFinance.Validation;

namespace SFA.DAS.EmployerFinance.Queries.GetContent;

public class GetContentRequestHandler : IRequestHandler<GetContentQuery, GetContentResponse>
{
    private readonly IValidator<GetContentQuery> _validator;
    private readonly ILogger<GetContentRequestHandler> _logger;
    private readonly IContentApiClient _contentApiClient;
    private readonly EmployerFinanceWebConfiguration _configuration;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAssociatedAccountsService _associatedAccountsService;

    public GetContentRequestHandler(
        IValidator<GetContentQuery> validator,
        ILogger<GetContentRequestHandler> logger,
        IContentApiClient contentApiClient,
        EmployerFinanceWebConfiguration configuration,
        IHttpContextAccessor httpContextAccessor,
        IAssociatedAccountsService associatedAccountsService)
    {
        _validator = validator;
        _logger = logger;
        _contentApiClient = contentApiClient;
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
        _associatedAccountsService = associatedAccountsService;
    }

    public async Task<GetContentResponse> Handle(GetContentQuery message, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(message);

        if (!validationResult.IsValid())
        {
            throw new InvalidRequestException(validationResult.ValidationDictionary);
        }

        var hashedAccountId = _httpContextAccessor.HttpContext.Request.RouteValues["HashedAccountId"]?.ToString().ToUpper();

        if (string.IsNullOrEmpty(hashedAccountId))
        {
            _logger.LogInformation("GetContentRequestHandler HashedAccountId not found on route.");
            return new GetContentResponse();
        }

        _logger.LogInformation("GetContentRequestHandler HashedAccountId: {Id}.", hashedAccountId);

        var levyStatus = await GetAccountLevyStatus(hashedAccountId);

        var applicationId = $"{_configuration.ApplicationId}-{levyStatus.ToString().ToLower()}";

        _logger.LogInformation("GetContentRequestHandler Fetching ContentBanner for applicationId: '{ApplicationId}'.", applicationId);

        try
        {
            var contentBanner = await _contentApiClient.Get(message.ContentType, applicationId);

            _logger.LogInformation("GetContentRequestHandler ContentBanner data: '{ContentBanner}'.", contentBanner);

            return new GetContentResponse
            {
                Content = contentBanner
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetContentRequestHandler Failed to get Content {ContentType} for {ApplicationId}", message.ContentType, applicationId);

            return new GetContentResponse
            {
                HasFailed = true
            };
        }
    }

    private async Task<ApprenticeshipEmployerType> GetAccountLevyStatus(string hashedAccountId)
    {
        var associatedAccounts = await _associatedAccountsService.GetAccounts(forceRefresh: false);

        var hasEmployerAccountsClaims = associatedAccounts.TryGetValue(hashedAccountId, out var employerAccount);

        return hasEmployerAccountsClaims ? (ApprenticeshipEmployerType)employerAccount.ApprenticeshipEmployerType : ApprenticeshipEmployerType.Unknown;
    }
}