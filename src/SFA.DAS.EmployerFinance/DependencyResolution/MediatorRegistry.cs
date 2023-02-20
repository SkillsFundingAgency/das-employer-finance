using StructureMap;

namespace SFA.DAS.EmployerFinance.DependencyResolution;

public class MediatorRegistry : Registry
{
    public MediatorRegistry()
    {
        For<IMediator>().Use<Mediator>();
    }
}