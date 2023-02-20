using SFA.DAS.EmployerFinance.MarkerInterfaces;
using SFA.DAS.Encoding;
using SFA.DAS.UnitOfWork.Context;

namespace SFA.DAS.EmployerFinance.Models;

public abstract class Entity
{
    protected IEncodingService _encodingService;
    protected IPublicHashingService _publicHashingService;

    protected void Publish<T>(Action<T> action) where T : new()
    {
        UnitOfWorkContext.AddEvent<object>(() =>
        {
            var message = new T();
            action(message);
            return message;
        });
    }

    public IEncodingService EncodingService
    {
        set { _encodingService = value; }
    }

    public IPublicHashingService PublicHashingService
    {
        set { _publicHashingService = value; }
    }
}