using HMRC.ESFA.Levy.Api.Types.Exceptions;
using Polly;

namespace SFA.DAS.EmployerFinance.Policies.Hmrc;

[PolicyName(Name)]
public class HmrcExecutionPolicy : ExecutionPolicy
{
    public const string Name = "HMRC Policy";

    private readonly ILogger<HmrcExecutionPolicy> _logger;

    public HmrcExecutionPolicy(ILogger<HmrcExecutionPolicy> logger) : this(logger, new TimeSpan(0, 0, 2))
    {
    }

    public HmrcExecutionPolicy(ILogger<HmrcExecutionPolicy> logger, TimeSpan retryWaitTime)
    {
        _logger = logger;

        var tooManyRequestsPolicy = CreateAsyncRetryPolicy<ApiHttpException>(ex => ex.HttpCode.Equals(429), 1, TimeSpan.FromSeconds(10), OnRetryableFailure);
        var serviceUnavailablePolicy = CreateAsyncRetryPolicy<ApiHttpException>(ex => ex.HttpCode.Equals(503), 3, retryWaitTime, OnRetryableFailure);
        var internalServerErrorPolicy = CreateAsyncRetryPolicy<ApiHttpException>(ex => ex.HttpCode.Equals(500), 3, retryWaitTime, OnRetryableFailure);

        RootPolicy = Policy.WrapAsync(tooManyRequestsPolicy, serviceUnavailablePolicy, internalServerErrorPolicy);
    }

    protected override T OnException<T>(Exception ex)
    {
        if (ex is ApiHttpException exception)
        {
            _logger.LogInformation("ApiHttpException - {Ex}", ex);

            switch (exception.HttpCode)
            {
                case 404:
                    _logger.LogInformation("Resource not found - {Ex}", ex);
                    return default;
            }
        }

        _logger.LogError(ex, "Exceeded retry limit - {Ex}", ex);
        throw ex;
    }

    private void OnRetryableFailure(Exception ex)
    {
        _logger.LogInformation("Error calling HMRC - {Ex} - Will retry", ex);
    }
}

