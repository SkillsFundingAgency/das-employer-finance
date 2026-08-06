using SFA.DAS.EmployerFinance.Queries.GetEnglishFractionCurrent;

namespace SFA.DAS.EmployerFinance.UnitTests.Queries.GetEnglishFractionCurrentTests;

public class WhenValidatingTheQuery
{
    private GetEnglishFractionCurrentQueryValidator _validator;

    [SetUp]
    public void Arrange()
    {
        _validator = new GetEnglishFractionCurrentQueryValidator();
    }

    [Test]
    public void ThenTrueIsReturnedWhenFieldsAreValid()
    {
        var query = new GetEnglishFractionCurrentQuery
        {
            HashedAccountId = "ABC123",
            EmpRefs = ["123/AB456", "456CD789"]
        };

        var actual = _validator.Validate(query);

        actual.IsValid().Should().BeTrue();
        query.EmpRefs.Should().BeEquivalentTo(["123/AB456", "456/CD789"]);
    }

    [Test]
    public void ThenFalseIsReturnedWhenAnEmpRefIsInvalid()
    {
        var actual = _validator.Validate(new GetEnglishFractionCurrentQuery
        {
            HashedAccountId = "ABC123",
            EmpRefs = ["123/AB456", "778<>/GDS00004"]
        });

        actual.IsValid().Should().BeFalse();
        actual.ValidationDictionary.Should().Contain(new KeyValuePair<string, string>(
            "EmpRefs",
            "EmpRefs[1] must be a valid PAYE reference"));
    }
}
