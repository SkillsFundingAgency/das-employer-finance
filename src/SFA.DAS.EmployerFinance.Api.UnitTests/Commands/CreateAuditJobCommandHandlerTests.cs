using SFA.DAS.EmployerFinance.Commands.CreateAuditJob;
using SFA.DAS.EmployerFinance.Data.Contracts;

namespace SFA.DAS.EmployerFinance.Api.UnitTests.Commands;

public class CreateAuditJobCommandHandlerTests
{
    [Test]
    public async Task Then_Returns_Not_Created_When_Job_Already_Exists()
    {
        var repository = new Mock<IAuditLogRepository>();
        repository.Setup(x => x.JobExists("job-1", It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var handler = new CreateAuditJobCommandHandler(repository.Object);

        var result = await handler.Handle(new CreateAuditJobCommand
        {
            JobId = "job-1",
            Description = "Import Payments",
            DateStarted = DateTime.UtcNow,
            NumRecords = 1
        }, CancellationToken.None);

        result.Created.Should().BeFalse();
    }

    [Test]
    public async Task Then_Creates_Job_When_It_Does_Not_Exist()
    {
        var repository = new Mock<IAuditLogRepository>();
        repository.Setup(x => x.JobExists("job-1", It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var handler = new CreateAuditJobCommandHandler(repository.Object);

        var result = await handler.Handle(new CreateAuditJobCommand
        {
            JobId = "job-1",
            Description = "Import Payments",
            DateStarted = DateTime.UtcNow,
            NumRecords = 1
        }, CancellationToken.None);

        result.Created.Should().BeTrue();
        repository.Verify(x => x.CreateJob(It.IsAny<Models.AuditLogs.JobRun>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
