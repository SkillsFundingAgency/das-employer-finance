using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerFinance.Web.Extensions;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.EmployerFinance.Web.UnitTests.Extensions;

public class ConfigurationExtensionsTests
{
    [Test]
    [MoqInlineAutoData("true", true)]
    [MoqInlineAutoData("false", false)]
    [MoqInlineAutoData(null, false)]
    public void UseGovUkSignIn_WhenConfigValue_ReturnCorrectValue(string configValue, bool expected)
    {
        // Arrange
        var configuration = new Mock<IConfiguration>();
        configuration.Setup(x => x["EmployerFinanceConfiguration:UseGovSignIn"]).Returns(configValue);

        // Act
        var result = configuration.Object.UseGovUkSignIn();

        // Assert
        result.Should().Be(expected);
    }

}