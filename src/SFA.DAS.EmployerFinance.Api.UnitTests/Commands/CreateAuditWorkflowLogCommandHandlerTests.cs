using SFA.DAS.EmployerFinance.Commands.CreateAuditWorkflowLog;
using SFA.DAS.EmployerFinance.Data.Contracts;

namespace SFA.DAS.EmployerFinance.Api.UnitTests.Commands;

public class CreateAuditWorkflowLogCommandHandlerTests
{
    [Test]
    public async Task Then_Returns_Failure_When_Job_Does_Not_Exist()
    {
        var repository = new Mock<IAuditLogRepository>();
        repository.Setup(x => x.JobExists("job-1", It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var handler = new CreateAuditWorkflowLogCommandHandler(repository.Object);

        var result = await handler.Handle(new CreateAuditWorkflowLogCommand
        {
            JobId = "job-1",
            WorkflowId = "workflow-1",
            Sequence = 1
        }, CancellationToken.None);

        result.Succeeded.Should().BeFalse();
    }

    [Test]
    public async Task Then_Ignores_Duplicate_Sequence()
    {
        var repository = new Mock<IAuditLogRepository>();
        repository.Setup(x => x.JobExists("job-1", It.IsAny<CancellationToken>())).ReturnsAsync(true);
        repository.Setup(x => x.WorkflowLogExists("workflow-1", 1, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var handler = new CreateAuditWorkflowLogCommandHandler(repository.Object);

        var result = await handler.Handle(new CreateAuditWorkflowLogCommand
        {
            JobId = "job-1",
            WorkflowId = "workflow-1",
            Sequence = 1
        }, CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.DuplicateIgnored.Should().BeTrue();
    }

    [Test]
    public async Task Then_Creates_Workflow_Log_When_Request_Is_Valid()
    {
        var repository = new Mock<IAuditLogRepository>();
        repository.Setup(x => x.JobExists("job-1", It.IsAny<CancellationToken>())).ReturnsAsync(true);
        repository.Setup(x => x.WorkflowLogExists("workflow-1", 1, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var handler = new CreateAuditWorkflowLogCommandHandler(repository.Object);

        var result = await handler.Handle(new CreateAuditWorkflowLogCommand
        {
            JobId = "job-1",
            WorkflowId = "workflow-1",
            SpanId = "12345",
            Sequence = 1,
            Stage = "Start",
            Description = "Started import",
            Status = "started",
            DataJson = "{}"
        }, CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        repository.Verify(x => x.CreateWorkflowLog(It.IsAny<Models.AuditLogs.WorkflowLog>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
