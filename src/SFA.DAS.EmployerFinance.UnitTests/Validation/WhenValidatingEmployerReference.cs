using SFA.DAS.EmployerFinance.Validation;

namespace SFA.DAS.EmployerFinance.UnitTests.Validation;

public class WhenValidatingEmployerReference
{
    [TestCase("123/AB456")]
    [TestCase("123AB456")]
    [TestCase("123ab456")]
    [TestCase(" 123/AB456 ")]
    public void ThenTrueIsReturnedForValidPayeReferences(string employerReference)
    {
        var result = EmployerReferenceValidation.TryNormalise(employerReference, out var normalised);

        result.Should().BeTrue();
        normalised.Should().Be("123/AB456");
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase(" ")]
    [TestCase("778<>/GDS00004")]
    [TestCase("12/AB456")]
    [TestCase("1234/AB456")]
    [TestCase("123/ABCDEFGH")]
    public void ThenFalseIsReturnedForInvalidPayeReferences(string employerReference)
    {
        var result = EmployerReferenceValidation.TryNormalise(employerReference, out var normalised);

        result.Should().BeFalse();
        normalised.Should().BeEmpty();
    }

    [Test]
    public void ThenFalseIsReturnedWhenReferenceContainsControlCharacters()
    {
        var result = EmployerReferenceValidation.TryNormalise("123/AB456\0", out var normalised);

        result.Should().BeFalse();
        normalised.Should().BeEmpty();
    }

    [Test]
    public void ThenFalseIsReturnedWhenReferenceExceedsMaximumLength()
    {
        var employerReference = new string('1', 51);

        var result = EmployerReferenceValidation.TryNormalise(employerReference, out var normalised);

        result.Should().BeFalse();
        normalised.Should().BeEmpty();
    }
}
