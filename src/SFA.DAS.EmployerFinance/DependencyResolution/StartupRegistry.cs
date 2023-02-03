using SFA.DAS.EmployerFinance.Startup;
using StructureMap;

namespace SFA.DAS.EmployerFinance.DependencyResolution
{
    public class StartupRegistry :Registry
    {
        public StartupRegistry()
        {
            Scan(s =>
            {
                s.AssembliesAndExecutablesFromApplicationBaseDirectory(a => a.GetName().Name.StartsWith("SFA.DAS"));
                //MAP-192 need checking
                //s.Convention<CompositeDecorator<DefaultStartup, IStartup>>();
            });
        }
    }
}
