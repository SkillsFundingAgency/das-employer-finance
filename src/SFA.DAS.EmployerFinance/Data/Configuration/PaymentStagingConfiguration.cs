using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.EmployerFinance.Models.PaymentStaging
{
    [ExcludeFromCodeCoverage]
    public class PaymentStagingConfiguration : IEntityTypeConfiguration<PaymentStagingModel>
    {
        public void Configure(EntityTypeBuilder<PaymentStagingModel> builder)
        {
            builder.ToTable("PaymentStaging", "employer_financial");

            // Primary Key
            builder.HasKey(x => x.PaymentId);

            builder.Property(x => x.PaymentId)
                .IsRequired();

            builder.Property(x => x.AccountId)
                .IsRequired();

            builder.Property(x => x.Ukprn)
                .IsRequired();

            builder.Property(x => x.Uln)
                .IsRequired();

            builder.Property(x => x.ApprenticeshipId)
                .IsRequired();

            builder.Property(x => x.CollectionPeriodId)
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(x => x.DeliveryPeriodMonth)
                .IsRequired();

            builder.Property(x => x.DeliveryPeriodYear)
                .IsRequired();

            builder.Property(x => x.CollectionPeriodMonth)
                .IsRequired();

            builder.Property(x => x.CollectionPeriodYear)
                .IsRequired();

            builder.Property(x => x.FundingSource)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.TransactionType)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.Amount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(x => x.EvidenceSubmittedOn)
                .IsRequired();

            builder.Property(x => x.EmployerAccountVersion)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.ApprenticeshipVersion)
                .HasMaxLength(50)
                .IsRequired();

            builder.HasIndex(x => x.PaymentId)
                .IsUnique();

            builder.HasIndex(x => new { x.CollectionPeriodId, x.AccountId });
        }
    }
}