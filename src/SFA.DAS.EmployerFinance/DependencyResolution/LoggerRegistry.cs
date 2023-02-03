using StructureMap;
using SFA.DAS.NLog.Logger;

namespace SFA.DAS.EmployerFinance.DependencyResolution
{
    public class LoggerRegistry :Registry
    {
        public LoggerRegistry()
        {
            For<ILog>().Use(c => new NLogLogger(c.ParentType, c.GetInstance<ILoggingContext>(), null)).AlwaysUnique();
        }
    }
}
