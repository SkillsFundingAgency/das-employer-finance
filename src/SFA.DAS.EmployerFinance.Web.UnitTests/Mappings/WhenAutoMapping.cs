using AutoMapper;
using SFA.DAS.EmployerFinance.Mappings;

namespace SFA.DAS.EmployerFinance.Web.UnitTests.Mappings;

[TestFixture]
public class WhenAutoMapping
{
    [Test]
    public void ThenShouldUseValidConfiguration()
    {
        var config = new MapperConfiguration(c => c.AddProfiles(new List<AutoMapper.Profile>{new HealthCheckMappings()}));

        config.AssertConfigurationIsValid();
    }
}