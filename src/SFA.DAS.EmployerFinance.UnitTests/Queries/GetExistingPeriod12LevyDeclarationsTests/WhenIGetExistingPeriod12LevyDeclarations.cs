using SFA.DAS.EmployerFinance.Api.Types;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Extensions;
using SFA.DAS.EmployerFinance.Interfaces;
using SFA.DAS.EmployerFinance.Queries.GetExistingPeriod12LevyDeclarations;
using SFA.DAS.EmployerFinance.UnitTests.Queries;
using SFA.DAS.EmployerFinance.Validation;

namespace SFA.DAS.EmployerFinance.UnitTests.Queries.GetExistingPeriod12LevyDeclarationsTests;

public class WhenIGetExistingPeriod12LevyDeclarations
    : QueryBaseTest<GetExistingPeriod12LevyDeclarationsQueryHandler, GetExistingPeriod12LevyDeclarationsQuery,
        List<ExistingPeriod12LevyDeclarationResult>>
{
    private Mock<IDasLevyRepository> _dasLevyRepository;
    private Mock<ICurrentDateTime> _currentDateTime;
    private const string ExpectedEmpRef = "45TGB";
    private static readonly DateTime FixedNow = new(2026, 5, 12, 12, 0, 0, DateTimeKind.Utc);

    public override GetExistingPeriod12LevyDeclarationsQuery Query { get; set; }
    public override GetExistingPeriod12LevyDeclarationsQueryHandler RequestHandler { get; set; }
    public override Mock<IValidator<GetExistingPeriod12LevyDeclarationsQuery>> RequestValidator { get; set; }

    [SetUp]
    public void Arrange()
    {
        SetUp();

        _dasLevyRepository = new Mock<IDasLevyRepository>();
        _currentDateTime = new Mock<ICurrentDateTime>();
        _currentDateTime.Setup(x => x.Now).Returns(FixedNow);

        Query = new GetExistingPeriod12LevyDeclarationsQuery { EmpRef = ExpectedEmpRef };

        _dasLevyRepository
            .Setup(x => x.GetExistingPeriod12LevyDeclarations(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<short>()))
            .ReturnsAsync(new List<ExistingPeriod12LevyDeclarationResult>());

        RequestHandler = new GetExistingPeriod12LevyDeclarationsQueryHandler(
            RequestValidator.Object,
            _dasLevyRepository.Object,
            _currentDateTime.Object);
    }

    [Test]
    public override async Task ThenIfTheMessageIsValidTheRepositoryIsCalled()
    {
        var expectedPayrollYear = FixedNow.ToPayrollYearString();
        _dasLevyRepository
            .Setup(x => x.GetExistingPeriod12LevyDeclarations(ExpectedEmpRef, expectedPayrollYear, 12))
            .ReturnsAsync(new List<ExistingPeriod12LevyDeclarationResult>());

        await RequestHandler.Handle(Query, CancellationToken.None);

        _dasLevyRepository.Verify(x => x.GetExistingPeriod12LevyDeclarations(ExpectedEmpRef, expectedPayrollYear, 12));
    }

    [Test]
    public override async Task ThenIfTheMessageIsValidTheValueIsReturnedInTheResponse()
    {
        var expectedPayrollYear = FixedNow.ToPayrollYearString();
        var expected = new List<ExistingPeriod12LevyDeclarationResult>
        {
            new()
            {
                Id = "999",
                LevyDueYtd = 100m,
                SubmissionDate = new DateTime(2026, 4, 1),
                PayrollYear = expectedPayrollYear,
                PayrollMonth = 12,
                SubmissionId = 888
            }
        };

        _dasLevyRepository
            .Setup(x => x.GetExistingPeriod12LevyDeclarations(ExpectedEmpRef, expectedPayrollYear, 12))
            .ReturnsAsync(expected);

        var actual = await RequestHandler.Handle(Query, CancellationToken.None);

        actual.Should().BeEquivalentTo(expected, options => options.WithStrictOrdering());
    }
}
