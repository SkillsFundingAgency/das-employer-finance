using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Queries.GetCeasedPayeSchemesByAccount;

namespace SFA.DAS.EmployerFinance.UnitTests.Queries.GetCeasedPayeSchemesByAccountIdTests;

[TestFixture]
public class WhenIGetCeasedPayeSchemesByAccountId
{
    private const long AccountId = 12345;

    private Mock<IDasLevyRepository> _dasLevyRepository;
    private GetCeasedPayeSchemesByAccountIdQueryHandler _handler;

    [SetUp]
    public void Arrange()
    {
        _dasLevyRepository = new Mock<IDasLevyRepository>();
        _handler = new GetCeasedPayeSchemesByAccountIdQueryHandler(_dasLevyRepository.Object);
    }

    [Test]
    public async Task Then_The_Ceased_Paye_Schemes_For_The_Account_Are_Returned()
    {
        _dasLevyRepository.Setup(x => x.GetAccountCeasedPayeSchemes(AccountId)).ReturnsAsync(["123/AB12345", "456/CD67890"]);

        var result = await _handler.Handle(new GetCeasedPayeSchemesByAccountIdQuery(AccountId), CancellationToken.None);

        result.PayeSchemes.Should().BeEquivalentTo("123/AB12345", "456/CD67890");
    }

    [Test]
    public async Task Then_An_Empty_List_Is_Returned_When_There_Are_No_Ceased_Paye_Schemes()
    {
        _dasLevyRepository.Setup(x => x.GetAccountCeasedPayeSchemes(AccountId)).ReturnsAsync([]);

        var result = await _handler.Handle(new GetCeasedPayeSchemesByAccountIdQuery(AccountId), CancellationToken.None);

        result.PayeSchemes.Should().NotBeNull().And.BeEmpty();
    }
}
