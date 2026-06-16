using global::SFA.DAS.EmployerFinance.Models.Transfers;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.EmployerFinance.Data.Configuration;

[ExcludeFromCodeCoverage]
public class TransferStagingConfiguration : IEntityTypeConfiguration<TransferStaging>
{
    public void Configure(EntityTypeBuilder<TransferStaging> builder)
    {
        builder.ToTable("TransferStaging");

        builder.HasKey(x => x.TransferId);

        builder.Property(x => x.TransferId)
            .IsRequired();

        builder.Property(x => x.SenderAccountId)
            .IsRequired();

        builder.Property(x => x.ReceiverAccountId)
            .IsRequired();

        builder.Property(x => x.ReceiverAccountName)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.Amount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(x => x.TransferDate)
            .IsRequired();

        builder.Property(x => x.PeriodEnd)
            .HasMaxLength(25)
            .IsRequired();

        builder.Property(x => x.CollectionPeriodMonth)
            .IsRequired();

        builder.Property(x => x.CollectionPeriodYear)
            .IsRequired();

        builder.Property(x => x.Ukprn)
            .IsRequired();

        builder.Property(x => x.CourseName)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.CreatedBy)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.CorrelationId)
            .HasMaxLength(100);

        builder.HasIndex(x => x.TransferId)
            .IsUnique();

        builder.HasIndex(x => new { x.SenderAccountId, x.PeriodEnd });
    }
}
