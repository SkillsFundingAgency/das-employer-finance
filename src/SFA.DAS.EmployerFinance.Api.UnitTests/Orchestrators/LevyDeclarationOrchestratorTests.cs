using SFA.DAS.EmployerFinance.Api.Orchestrators;
using SFA.DAS.EmployerFinance.Api.Types;
using SFA.DAS.EmployerFinance.Commands.PersistLevyDeclarations;
using SFA.DAS.EmployerFinance.Models.Levy;
using SFA.DAS.EmployerFinance.Queries.GetExistingPeriod12LevyDeclarations;
using SFA.DAS.EmployerFinance.Queries.GetLastLevyDeclaration;
using SFA.DAS.EmployerFinance.Queries.GetLevyDeclarationSubmissionIds;

namespace SFA.DAS.EmployerFinance.Api.UnitTests.Orchestrators;

public class LevyDeclarationOrchestratorTests
{
    [Test]
    public async Task Then_PersistLevyDeclarations_Sends_Command_And_Returns_Response()
    {
        var request = new PersistLevyDeclarationRequestData
        {
            AccountId = 5,
            EmpRef = "999/ZZ",
            Declarations = new List<NormalizedLevyDeclaration>()
        };
        var expected = new PersistLevyDeclarationsResponse
        {
            DeclarationsPersisted = 0,
            DeclarationsSkipped = 0,
            TransactionsCreated = 0
        };
        var mediator = new Mock<IMediator>();
        var logger = new Mock<ILogger<LevyDeclarationOrchestrator>>();

        mediator
            .Setup(x => x.Send(It.Is<PersistLevyDeclarationsCommand>(c => c.Data == request), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var sut = new LevyDeclarationOrchestrator(mediator.Object, logger.Object);

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

        mediator.Setup(x => x.Send(
                It.Is<GetExistingPeriod12LevyDeclarationsQuery>(q => q.EmpRef == empRef),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var sut = new LevyDeclarationOrchestrator(mediator.Object, logger.Object);

        var result = await sut.GetExistingPeriod12LevyDeclarations(empRef);

        result.Should().BeEquivalentTo(expected, options => options.WithStrictOrdering());
    }

    [Test]
    public async Task Then_GetSubmissionIds_Returns_List_From_Mediator()
    {
        var empRef = "123/AB12345";
        var expectedIds = new List<long> { 10, 20, 30 };
        var mediator = new Mock<IMediator>();
        var logger = new Mock<ILogger<LevyDeclarationOrchestrator>>();

        mediator.Setup(x => x.Send(
                It.Is<GetLevyDeclarationSubmissionIdsQuery>(q => q.EmpRef == empRef),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedIds);

        var sut = new LevyDeclarationOrchestrator(mediator.Object, logger.Object);

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

        mediator.Setup(x => x.Send(
                It.Is<GetLastLevyDeclarationQuery>(q => q.EmpRef == empRef),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetLastLevyDeclarationResponse
            {
                Transaction = new DasDeclaration { SubmissionDate = submissionDate }
            });

        var sut = new LevyDeclarationOrchestrator(mediator.Object, logger.Object);

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

        mediator.Setup(x => x.Send(
                It.Is<GetLastLevyDeclarationQuery>(q => q.EmpRef == empRef),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetLastLevyDeclarationResponse)null!);

        var sut = new LevyDeclarationOrchestrator(mediator.Object, logger.Object);

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

        mediator.Setup(x => x.Send(
                It.Is<GetLastLevyDeclarationQuery>(q => q.EmpRef == empRef),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetLastLevyDeclarationResponse
            {
                Transaction = new DasDeclaration { SubmissionDate = DateTime.MinValue }
            });

        var sut = new LevyDeclarationOrchestrator(mediator.Object, logger.Object);

        var result = await sut.GetLastSubmissionDate(empRef);

        result.Should().NotBeNull();
        result.LastSumissionDate.Should().BeNull();
    }
}
