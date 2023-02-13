using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerFinance.Web.Authentication;
using SFA.DAS.EmployerFinance.Web.StartupExtensions;

namespace SFA.DAS.EmployerFinance.Web.UnitTests.AppStart;

public class WhenAddingServicesToTheContainer
{
    [Test]
    public void Then_Resolves_Authorization_Handlers()
    {
        var serviceCollection = new ServiceCollection();
        SetupServiceCollection(serviceCollection);
        var provider = serviceCollection.BuildServiceProvider();
            
        var type = provider.GetServices(typeof(IAuthorizationHandler)).ToList();
            
        Assert.IsNotNull(type);
        type.Count.Should().Be(3);
        type.Should().ContainSingle(c => c.GetType() == typeof(EmployerAccountAllRolesAuthorizationHandler));
        type.Should().ContainSingle(c => c.GetType() == typeof(EmployerAccountOwnerAuthorizationHandler));
    }


    private static void SetupServiceCollection(ServiceCollection serviceCollection)
    {
        var configuration = GenerateConfiguration();
            
        serviceCollection.AddSingleton(Mock.Of<IWebHostEnvironment>());
        serviceCollection.AddSingleton(Mock.Of<IConfiguration>());
        serviceCollection.AddConfigurationOptions(configuration);
        serviceCollection.AddDistributedMemoryCache();
        serviceCollection.AddApplicationServices();
        serviceCollection.AddAuthenticationServices();
    }
    
    private static IConfigurationRoot GenerateConfiguration()
    {
        var configSource = new MemoryConfigurationSource
        {
            InitialData = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("EmployerFinanceConfiguration:EmployerFinanceOuterApiConfiguration:BaseUrl", "https://test.com/"),
                new KeyValuePair<string, string>("EmployerFinanceConfiguration:EmployerFinanceOuterApiConfiguration:Key", "123edc"),
                new KeyValuePair<string, string>("EnvironmentName", "test"),
            }
        };

        var provider = new MemoryConfigurationProvider(configSource);

        return new ConfigurationRoot(new List<IConfigurationProvider> { provider });
    }
}