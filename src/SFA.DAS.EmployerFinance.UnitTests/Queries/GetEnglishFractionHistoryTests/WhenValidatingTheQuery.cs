using SFA.DAS.EmployerFinance.Queries.GetEnglishFractionHistory;

namespace SFA.DAS.EmployerFinance.UnitTests.Queries.GetEnglishFractionHistoryTests;

public class WhenValidatingTheQuery
{
    private GetEnglishFractionHistoryQueryValidator _validator;

    [SetUp]
    public void Arrange()
    {
        _validator = new GetEnglishFractionHistoryQueryValidator();
    }

    [Test]
    public void ThenTrueIsReturnedWhenFieldsAreValid()
    {
        var query = new GetEnglishFractionHistoryQuery
        {
            HashedAccountId = "ABC123",
            EmpRef = "123/AB456"
        };

        var actual = _validator.Validate(query);

        actual.IsValid().Should().BeTrue();
        query.EmpRef.Should().Be("123/AB456");
    }

    [Test]
    public void ThenEmpRefIsNormalisedWhenValidWithoutSlash()
    {
        var query = new GetEnglishFractionHistoryQuery
        {
            HashedAccountId = "ABC123",
            EmpRef = "123ab456"
        };

        var actual = _validator.Validate(query);

        actual.IsValid().Should().BeTrue();
        query.EmpRef.Should().Be("123/AB456");
    }

    [Test]
    public void ThenFalseIsReturnedWhenEmpRefIsInvalid()
    {
        var actual = _validator.Validate(new GetEnglishFractionHistoryQuery
        {
            HashedAccountId = "ABC123",
            EmpRef = "778<>/GDS00004"
        });

        actual.IsValid().Should().BeFalse();
        actual.ValidationDictionary.Should().Contain(new KeyValuePair<string, string>(
            "EmpRef",
            "EmpRef must be a valid PAYE reference"));
    }
}
