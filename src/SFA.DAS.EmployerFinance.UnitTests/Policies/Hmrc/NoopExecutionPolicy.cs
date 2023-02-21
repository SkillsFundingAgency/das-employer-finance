using SFA.DAS.EmployerFinance.Policies.Hmrc;

namespace SFA.DAS.EmployerFinance.UnitTests.Policies.Hmrc;

public class NoopExecutionPolicy : ExecutionPolicy
{
    public override void Execute(Action action)
    {
    }

    public override Task ExecuteAsync(Func<Task> action)
    {
        return action();
    }

    public override T Execute<T>(Func<T> func)
    {
        return func();
    }

    public override Task<T> ExecuteAsync<T>(Func<Task<T>> func)
    {
        return func();
    }
}