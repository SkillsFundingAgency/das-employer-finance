using System.Reflection;
using AutoMapper;
using SFA.DAS.EmployerFinance.Api.Controllers;
using SFA.DAS.EmployerFinance.Api.Orchestrators;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerFinance.Api.UnitTests.Controllers.AccountLevyControllerTests;

public class AccountLevyControllerTests
{
    protected FinanceLevyController Controller;
    protected Mock<IMediator> Mediator;
    protected Mock<ILogger<FinanceOrchestrator>> Logger;
    protected IMapper Mapper;
    protected Mock<IEncodingService> EncodingService;

    [SetUp]
    public void Arrange()
    {
        Mediator = new Mock<IMediator>();
        Logger = new Mock<ILogger<FinanceOrchestrator>>();
        EncodingService = new Mock<IEncodingService>();            
            
        Mapper = ConfigureMapper();
        var orchestrator = new FinanceOrchestrator(Mediator.Object, Logger.Object, Mapper, EncodingService.Object);
        Controller = new FinanceLevyController(orchestrator);
    }

    private static IMapper ConfigureMapper()
    {
        var profiles = Assembly.Load($"SFA.DAS.EmployerFinance.Api")
            .GetTypes()
            .Where(t => typeof(Profile).IsAssignableFrom(t))
            .Select(t => (Profile)Activator.CreateInstance(t))
            .ToList();

        var config = new MapperConfiguration(c =>
        {
            profiles.ForEach(c.AddProfile);
        });

        return config.CreateMapper();
    }
}