using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.EmployerFinance.Models.Payments;

namespace SFA.DAS.EmployerFinance.Data.Configuration;

public class PaymentMetadataStagingConfiguration : IEntityTypeConfiguration<PaymentMetaDataStaging>
{
    public void Configure(EntityTypeBuilder<PaymentMetaDataStaging> builder)
    {
        builder.ToTable("PaymentMetaDataStaging");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ProviderName)
               .HasMaxLength(200);

        builder.Property(x => x.PathwayName)
               .HasMaxLength(200);

        builder.Property(x => x.ApprenticeshipCourseName)
               .HasMaxLength(200);

        builder.Property(x => x.ApprenticeName)
               .HasMaxLength(200);

        builder.Property(x => x.ApprenticeNINumber)
               .HasMaxLength(20);

        builder.Property(x => x.LearningType)
               .HasMaxLength(25);

        builder.Property(x => x.CourseCode)
               .HasMaxLength(25);

        builder.Property(x => x.CohortId);

        builder.Property(x => x.StandardCode);
        builder.Property(x => x.FrameworkCode);
        builder.Property(x => x.ProgrammeType);
        builder.Property(x => x.PathwayCode);
        builder.Property(x => x.ApprenticeshipCourseLevel);

        builder.Property(x => x.ApprenticeshipCourseStartDate);

        builder.Property(x => x.IsHistoricProviderName);
        builder.Property(x => x.CreatedBy);
        builder.Property(x => x.CorrelationId);
    }
}
