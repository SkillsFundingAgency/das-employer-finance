namespace SFA.DAS.EmployerFinance.MessageHandlers.DependencyResolution;

public class StructureMapJobActivator : IJobActivator
{
    private readonly IContainer _container;

    public StructureMapJobActivator(IContainer container)
    {
        _container = container;
    }

    public T CreateInstance<T>()
    {
        return _container.GetInstance<T>();
    }
}