using AutoFixture.NUnit3;
using SFA.DAS.EmployerFinance.Infrastructure.OuterApiResponses.UserAccounts;
using SFA.DAS.EmployerFinance.Models.UserAccounts;

namespace SFA.DAS.EmployerFinance.UnitTests.Models;

public class WhenCastingFromApiResponseToEmployerUserAccounts
{
    [Test, AutoData]
    public void Then_The_Values_Are_Mapped(GetUserAccountsResponse source)
    {
        //Arrange
        source.IsSuspended = true;

        //Act
        var actual = (EmployerUserAccounts)source;

        //Assert
        actual.Should().BeEquivalentTo(source, options => options.Excluding(x => x.UserAccounts));
        actual.EmployerAccounts.Should().BeEquivalentTo(source.UserAccounts, options => options.Excluding(x => x.ApprenticeshipEmployerType));
        actual.IsSuspended.Should().BeTrue();
    }

    [Test, AutoData]
    public void Then_If_No_Accounts_Then_Empty_List_Returned(GetUserAccountsResponse source)
    {
        //Arrange
        source.UserAccounts = null;

        //Act
        var actual = (EmployerUserAccounts)source;

        //Assert
        actual.Should().BeEquivalentTo(source, options => options.Excluding(x => x.UserAccounts));
        actual.EmployerAccounts.Should().BeEmpty();
    }

    [Test, AutoData]
    public void Then_If_No_Values_Null_Returned()
    {
        //Act
        var actual = (EmployerUserAccounts)((GetUserAccountsResponse)null);

        //Assert
        actual.FirstName.Should().BeNullOrEmpty();
        actual.LastName.Should().BeNullOrEmpty();
        actual.EmployerUserId.Should().BeNullOrEmpty();
        actual.EmployerAccounts.Should().BeEmpty();
        actual.IsSuspended.Should().BeFalse();
    }
}