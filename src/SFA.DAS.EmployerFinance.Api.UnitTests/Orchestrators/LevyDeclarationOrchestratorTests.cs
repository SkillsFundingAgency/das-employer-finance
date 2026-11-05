using AutoFixture.NUnit4;
using AutoMapper;
using SFA.DAS.EmployerFinance.Api.Orchestrators;
using SFA.DAS.EmployerFinance.Api.Types;
using SFA.DAS.EmployerFinance.Commands.PersistLevyDeclarations;
using SFA.DAS.EmployerFinance.Models.Levy;
using SFA.DAS.EmployerFinance.Queries.GetExistingPeriod12LevyDeclarations;
using SFA.DAS.EmployerFinance.Queries.GetLastLevyDeclaration;
using SFA.DAS.EmployerFinance.Queries.GetLevyDeclarationsByAccountAndDateRange;
using SFA.DAS.EmployerFinance.Queries.GetLevyDeclarationSubmissionIds;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.EmployerFinance.Api.UnitTests.Orchestrators;

public class LevyDeclarationOrchestratorTests
{
    [Test]
    public async Task Then_PersistLevyDeclarations_Sends_Command_And_Returns_Response()
    {
        var request = new PersistLevyDeclarationRequestData
        {
            CorrelationId = "corr-123",
            AccountId = 5,
            EmpRef = "999/ZZ",
            Declarations = new List<NormalizedLevyDeclaration>()
        };
        var expected = new PersistLevyDeclarationsResponse
        {
            DeclarationsReceived = 0,
            DeclarationsPersisted = 0,
            DeclarationsSkipped = 0,
            LevyTransactionValue = 0,
            TransactionsCreated = 0
        };
        var mediator = new Mock<IMediator>();
        var logger = new Mock<ILogger<LevyDeclarationOrchestrator>>();
        var mapper = new Mock<IMapper>();

        mediator
            .Setup(x => x.Send(It.Is<PersistLevyDeclarationsCommand>(c => c.Data == request), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var sut = new LevyDeclarationOrchestrator(mediator.Object, mapper.Object, logger.Object);

        var result = await sut.PersistLevyDeclarations(request);

        result.Should().BeSameAs(expected);
        mediator.Verify(x => x.Send(It.Is<PersistLevyDeclarationsCommand>(c => c.Data == request), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Then_GetExistingPeriod12LevyDeclarations_Returns_List_From_Mediator()
    {
        var empRef = "123/AB12345";
        var expected = new List<ExistingPeriod12LevyDeclarationResult>
        {
            new()
            {
                Id = "42",
                LevyDueYtd = 1m,
                SubmissionDate = new DateTime(2026, 1, 1),
                PayrollYear = "25-26",
                PayrollMonth = 12,
                SubmissionId = 7
            }
        };
        var mediator = new Mock<IMediator>();
        var logger = new Mock<ILogger<LevyDeclarationOrchestrator>>();
        var mapper = new Mock<IMapper>();

        mediator.Setup(x => x.Send(
                It.Is<GetExistingPeriod12LevyDeclarationsQuery>(q => q.EmpRef == empRef),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var sut = new LevyDeclarationOrchestrator(mediator.Object, mapper.Object, logger.Object);

        var result = await sut.GetExistingPeriod12LevyDeclarations(empRef);

        result.Should().BeEquivalentTo(expected, options => options.WithStrictOrdering());
    }

    [Test]
    public async Task Then_GetSubmissionIds_Returns_List_From_Mediator()
    {
        var empRef = "123/AB12345";
        var mediatorIds = new List<long> { 10L, 20L, 30L };
        var expectedIds = new List<string> { "10", "20", "30" };
        var mediator = new Mock<IMediator>();
        var logger = new Mock<ILogger<LevyDeclarationOrchestrator>>();
        var mapper = new Mock<IMapper>();

        mediator.Setup(x => x.Send(
                It.Is<GetLevyDeclarationSubmissionIdsQuery>(q => q.EmpRef == empRef),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(mediatorIds);

        var sut = new LevyDeclarationOrchestrator(mediator.Object, mapper.Object, logger.Object);

        var result = await sut.GetSubmissionIds(empRef);

        result.Should().BeEquivalentTo(expectedIds, options => options.WithStrictOrdering());
    }

    [Test]
    public async Task Then_Returns_SubmissionDate_Minus_One_Day_When_SubmissionDate_Is_Valid()
    {
        var empRef = "123/AB12345";
        var submissionDate = new DateTime(2026, 4, 10);
        var mediator = new Mock<IMediator>();
        var logger = new Mock<ILogger<LevyDeclarationOrchestrator>>();
        var mapper = new Mock<IMapper>();

        mediator.Setup(x => x.Send(
                It.Is<GetLastLevyDeclarationQuery>(q => q.EmpRef == empRef),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetLastLevyDeclarationResponse
            {
                Transaction = new DasDeclaration { SubmissionDate = submissionDate }
            });

        var sut = new LevyDeclarationOrchestrator(mediator.Object, mapper.Object, logger.Object);

        var result = await sut.GetLastSubmissionDate(empRef);

        result.Should().NotBeNull();
        result.LastSumissionDate.Should().Be(submissionDate.AddDays(-1));
    }

    [Test]
    public async Task Then_Returns_Null_When_Declaration_Is_Null()
    {
        var empRef = "123/AB12345";
        var mediator = new Mock<IMediator>();
        var logger = new Mock<ILogger<LevyDeclarationOrchestrator>>();
        var mapper = new Mock<IMapper>();

        mediator.Setup(x => x.Send(
                It.Is<GetLastLevyDeclarationQuery>(q => q.EmpRef == empRef),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetLastLevyDeclarationResponse)null!);

        var sut = new LevyDeclarationOrchestrator(mediator.Object, mapper.Object, logger.Object);

        var result = await sut.GetLastSubmissionDate(empRef);

        result.Should().NotBeNull();
        result.LastSumissionDate.Should().BeNull();
    }

    [Test]
    public async Task Then_Returns_Null_When_SubmissionDate_Is_MinValue()
    {
        var empRef = "123/AB12345";
        var mediator = new Mock<IMediator>();
        var logger = new Mock<ILogger<LevyDeclarationOrchestrator>>();
        var mapper = new Mock<IMapper>();

        mediator.Setup(x => x.Send(
                It.Is<GetLastLevyDeclarationQuery>(q => q.EmpRef == empRef),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetLastLevyDeclarationResponse
            {
                Transaction = new DasDeclaration { SubmissionDate = DateTime.MinValue }
            });

        var sut = new LevyDeclarationOrchestrator(mediator.Object, mapper.Object, logger.Object);

        var result = await sut.GetLastSubmissionDate(empRef);

        result.Should().NotBeNull();
        result.LastSumissionDate.Should().BeNull();
    }

    [Test, MoqAutoData]
    public async Task ThenReturnsDeclarations(
        long accountId,
        DateOnly fromDate,
        DateOnly toDate,
        GetLevyDeclarationsByAccountAndDateRangeQueryResult queryResult,
        List<LevyDeclaration> mappedDeclarations,
        [Frozen] Mock<IMediator> mediator,
        [Frozen] Mock<IMapper> mapper,
        [Greedy] LevyDeclarationOrchestrator sut)
    {
        // Arrange
        mediator
            .Setup(x => x.Send(
                It.Is<GetLevyDeclarationsByAccountAndDateRangeQuery>(q =>
                    q.AccountId == accountId &&
                    q.FromDate == fromDate &&
                    q.ToDate == toDate),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(queryResult);

        for (var i = 0; i < queryResult._.Count; i++)
        {
            var item = queryResult._[i];
            mapper
                .Setup(x => x.Map<LevyDeclaration>(item))
                .Returns(mappedDeclarations[i]);
        }

        // Act
        var result = await sut.GetLevyDeclarationsByAccountIdAndDateRange(accountId, fromDate, toDate);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(mappedDeclarations);
    }

    [Test, MoqAutoData]
    public async Task ThenReturnsNullWhenResponseIsNull(
        long accountId,
        DateOnly fromDate,
        DateOnly toDate,
        [Frozen] Mock<IMediator> mediator,
        [Frozen] Mock<IMapper> mapper,
        [Greedy] LevyDeclarationOrchestrator sut)
    {
        // Arrange
        mediator
            .Setup(x => x.Send(
                It.IsAny<GetLevyDeclarationsByAccountAndDateRangeQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetLevyDeclarationsByAccountAndDateRangeQueryResult)null);

        // Act
        var result = await sut.GetLevyDeclarationsByAccountIdAndDateRange(accountId, fromDate, toDate);

        // Assert
        result.Should().BeNull();
    }

    [Test, MoqAutoData]
    public async Task ThenReturnsNullWhenDeclarationsIsNull(
        long accountId,
        DateOnly fromDate,
        DateOnly toDate,
        [Frozen] Mock<IMediator> mediator,
        [Frozen] Mock<IMapper> mapper,
        [Greedy] LevyDeclarationOrchestrator sut)
    {
        // Arrange
        var queryResult = new GetLevyDeclarationsByAccountAndDateRangeQueryResult(null);

        mediator
            .Setup(x => x.Send(
                It.IsAny<GetLevyDeclarationsByAccountAndDateRangeQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(queryResult);

        // Act
        var result = await sut.GetLevyDeclarationsByAccountIdAndDateRange(accountId, fromDate, toDate);

        // Assert
        result.Should().BeNull();
    }
}
