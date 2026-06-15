using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.EmployerFinance.Models.AuditLogs;

namespace SFA.DAS.EmployerFinance.Data.Configuration;

public class JobRunConfiguration : IEntityTypeConfiguration<JobRun>
{
    public void Configure(EntityTypeBuilder<JobRun> builder)
    {
        builder.ToTable("JobRun");
        builder.HasKey(x => x.JobId);

        builder.Property(x => x.JobId).HasMaxLength(100);
        builder.Property(x => x.Description).HasMaxLength(256).IsRequired();
        builder.Property(x => x.DateStarted).IsRequired();
        builder.Property(x => x.NumRecords).IsRequired();

        builder.HasMany(x => x.WorkflowLogs)
            .WithOne(x => x.JobRun)
            .HasForeignKey(x => x.JobId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
