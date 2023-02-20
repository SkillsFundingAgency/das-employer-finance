namespace SFA.DAS.EmployerFinance.Startup;

public interface IStartup
{
    Task StartAsync();
    Task StopAsync();
}