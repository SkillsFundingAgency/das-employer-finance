namespace SFA.DAS.EmployerFinance.Api.UnitTests;

public static class ObjectExtensions
{
    public static bool IsEquivalentTo(this object source, object expectation)
    {
        try
        {
            source.Should().BeEquivalentTo(expectation);
        }
        catch (Exception)
        {
            return false;
        }

        return true;
    }
}