using SFA.DAS.UnitOfWork.Context;

namespace SFA.DAS.EmployerFinance.Models;

public abstract class Entity
{
    protected void Publish<T>(Action<T> action) where T : new()
    {
        UnitOfWorkContext.AddEvent(() =>
        {
            var message = new T();
            action(message);
            return message;
        });
    }
}