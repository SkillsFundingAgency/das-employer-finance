using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.EmployerFinance.Models.AuditLogs;

namespace SFA.DAS.EmployerFinance.Data.Configuration;

public class WorkflowLogConfiguration : IEntityTypeConfiguration<WorkflowLog>
{
    public void Configure(EntityTypeBuilder<WorkflowLog> builder)
    {
        builder.ToTable("WorkflowLog");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.JobId).HasMaxLength(100).IsRequired();
        builder.Property(x => x.WorkflowId).HasMaxLength(200).IsRequired();
        builder.Property(x => x.SpanId).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Sequence).IsRequired();
        builder.Property(x => x.Stage).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(512).IsRequired();
        builder.Property(x => x.Status).HasMaxLength(50).IsRequired();
        builder.Property(x => x.DataJson).HasColumnType("nvarchar(max)");
        builder.Property(x => x.ErrorCode).HasMaxLength(100);
        builder.Property(x => x.ErrorMessage).HasMaxLength(4000);
        builder.Property(x => x.Created).IsRequired();

        builder.HasIndex(x => new { x.WorkflowId, x.Sequence }).IsUnique();
        builder.HasIndex(x => x.JobId);
        builder.HasIndex(x => new { x.JobId, x.WorkflowId });
    }
}
