using AutoMapper;
using SFA.DAS.EmployerFinance.Mappings;

namespace SFA.DAS.EmployerFinance.UnitTests.Mappings
{
    [TestFixture]
    public class WhenAutoMapping
    {
        [Test]
        public void ThenShouldUseValidConfiguration()
        {
            var config = new MapperConfiguration(c => {
                c.AddProfile(typeof(HealthCheckMappings));
                c.AddProfile(typeof(PaymentMappings));
            });

            config.AssertConfigurationIsValid();
        }
    }
}