// This test utility file is intentionally left empty as we are removing the HMRC execution policy
// and relying on NServiceBus retries instead

using System;
using System.Threading.Tasks;
using SFA.DAS.EmployerFinance.Policies.Hmrc;

namespace SFA.DAS.EmployerFinance.UnitTests.Policies.Hmrc;

public class NoopExecutionPolicy : ExecutionPolicy
{

    public override Task ExecuteAsync(Func<Task> action)
    {
        return action();
    }


    public override Task<T> ExecuteAsync<T>(Func<Task<T>> func)
    {
        return func();
    }
}