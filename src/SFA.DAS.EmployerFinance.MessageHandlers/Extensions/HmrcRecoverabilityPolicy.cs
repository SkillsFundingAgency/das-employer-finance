using HMRC.ESFA.Levy.Api.Types.Exceptions;
using NServiceBus.Transport;

namespace SFA.DAS.EmployerFinance.MessageHandlers.Extensions;

public static class HmrcRecoverabilityPolicy
{
    public const int MaxDelayedRetries = 3;
    public static readonly TimeSpan TooManyRequestsDelay = TimeSpan.FromSeconds(30);
    public static readonly TimeSpan ForbiddenDelay = TimeSpan.FromMinutes(2);

    public static RecoverabilityAction Evaluate(RecoverabilityConfig config, ErrorContext context)
    {
        var apiException = FindHmrcRateLimitOrForbiddenException(context.Exception);
        if (apiException == null)
        {
            return DefaultRecoverabilityPolicy.Invoke(config, context);
        }

        if (context.DelayedDeliveriesPerformed >= MaxDelayedRetries)
        {
            return RecoverabilityAction.MoveToError(config.Failed.ErrorQueue);
        }

        var delay = apiException.HttpCode == 429 ? TooManyRequestsDelay : ForbiddenDelay;
        return RecoverabilityAction.DelayedRetry(delay);
    }

    private static ApiHttpException FindHmrcRateLimitOrForbiddenException(Exception exception)
    {
        if (exception == null)
        {
            return null;
        }

        if (exception is ApiHttpException apiException && apiException.HttpCode is 429 or 403)
        {
            return apiException;
        }

        if (exception is AggregateException aggregateException)
        {
            foreach (var inner in aggregateException.InnerExceptions)
            {
                var found = FindHmrcRateLimitOrForbiddenException(inner);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        return FindHmrcRateLimitOrForbiddenException(exception.InnerException);
    }
}
