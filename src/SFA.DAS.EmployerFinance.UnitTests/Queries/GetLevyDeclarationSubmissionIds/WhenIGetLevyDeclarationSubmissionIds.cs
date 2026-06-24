using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Queries.GetLevyDeclarationSubmissionIds;
using SFA.DAS.EmployerFinance.UnitTests.Queries;
using SFA.DAS.EmployerFinance.Validation;

namespace SFA.DAS.EmployerFinance.UnitTests.Queries.GetLevyDeclarationSubmissionIdsTests;

public class WhenIGetLevyDeclarationSubmissionIds
    : QueryBaseTest<GetLevyDeclarationSubmissionIdsQueryHandler, GetLevyDeclarationSubmissionIdsQuery, List<long>>
{
    private Mock<IDasLevyRepository> _dasLevyRepository;
    private const string ExpectedEmpRef = "45TGB";

    public override GetLevyDeclarationSubmissionIdsQuery Query { get; set; }
    public override GetLevyDeclarationSubmissionIdsQueryHandler RequestHandler { get; set; }
    public override Mock<IValidator<GetLevyDeclarationSubmissionIdsQuery>> RequestValidator { get; set; }

    [SetUp]
    public void Arrange()
    {
        SetUp();

        _dasLevyRepository = new Mock<IDasLevyRepository>();

        Query = new GetLevyDeclarationSubmissionIdsQuery { EmpRef = ExpectedEmpRef };

        RequestHandler = new GetLevyDeclarationSubmissionIdsQueryHandler(
            RequestValidator.Object,
            _dasLevyRepository.Object);
    }

    [Test]
    public override async Task ThenIfTheMessageIsValidTheRepositoryIsCalled()
    {
        _dasLevyRepository
            .Setup(x => x.GetEmployerDeclarationSubmissionIds(ExpectedEmpRef))
            .ReturnsAsync(new List<long>());

        await RequestHandler.Handle(Query, CancellationToken.None);

        _dasLevyRepository.Verify(x => x.GetEmployerDeclarationSubmissionIds(ExpectedEmpRef));
    }

    [Test]
    public override async Task ThenIfTheMessageIsValidTheValueIsReturnedInTheResponse()
    {
        var expected = new List<long> { 100, 200, 300 };
        _dasLevyRepository
            .Setup(x => x.GetEmployerDeclarationSubmissionIds(ExpectedEmpRef))
            .ReturnsAsync(expected);

        var actual = await RequestHandler.Handle(Query, CancellationToken.None);

        actual.Should().BeEquivalentTo(expected, options => options.WithStrictOrdering());
    }
}
