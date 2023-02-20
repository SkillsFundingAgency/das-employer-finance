using System.ComponentModel.DataAnnotations;
using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Interfaces;
using SFA.DAS.EmployerFinance.Validation;

namespace SFA.DAS.EmployerFinance.Queries.GetContent;

public class GetContentRequestHandler : IRequestHandler<GetContentRequest, GetContentResponse>
{
    private readonly IValidator<GetContentRequest> _validator;
    private readonly ILogger<GetContentRequestHandler> _logger;
    private readonly IContentApiClient _service;
    private readonly EmployerFinanceConfiguration _employerFinanceConfiguration;

    public GetContentRequestHandler(
        IValidator<GetContentRequest> validator,
        ILogger<GetContentRequestHandler> logger,
        IContentApiClient service,
        EmployerFinanceConfiguration employerFinanceConfiguration
    )
    {
        _validator = validator;
        _logger = logger;
        _service = service;
        _employerFinanceConfiguration = employerFinanceConfiguration;
    }

    public async Task<GetContentResponse> Handle(GetContentRequest message,CancellationToken cancellationToken)
    {
        var validationResult = _validator.Validate(message);

        if (!validationResult.IsValid())
        {
            throw new ValidationException(validationResult.ConvertToDataAnnotationsValidationResult(), null, null);
        }

        var applicationId = message.UseLegacyStyles ? _employerFinanceConfiguration.ApplicationId + "-legacy" : _employerFinanceConfiguration.ApplicationId;

        try
        {
            var content = await _service.Get(message.ContentType, applicationId);

            return new GetContentResponse
            {
                Content = content
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to get Content {message.ContentType} for {applicationId}");

            return new GetContentResponse
            {
                HasFailed = true
            };
        }
    }
}