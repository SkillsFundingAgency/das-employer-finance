using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.EmployerFinance.Models.Levy;

namespace SFA.DAS.EmployerFinance.Data.Configuration;

public class LevyDeclarationEntityConfiguration : IEntityTypeConfiguration<LevyDeclarationEntity>
{
    public void Configure(EntityTypeBuilder<LevyDeclarationEntity> builder)
    {
        builder.ToTable("LevyDeclaration");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("Id")
            .HasColumnType("bigint")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.AccountId).HasColumnName("AccountId").HasColumnType("bigint");
        builder.Property(x => x.EmpRef).HasColumnName("EmpRef").HasColumnType("nvarchar(50)");
        builder.Property(x => x.LevyDueYtd).HasColumnName("LevyDueYTD").HasColumnType("decimal(18, 4)");
        builder.Property(x => x.LevyAllowanceForYear).HasColumnName("LevyAllowanceForYear").HasColumnType("decimal(18, 4)");
        builder.Property(x => x.SubmissionDate).HasColumnName("SubmissionDate").HasColumnType("datetime");
        builder.Property(x => x.SubmissionId).HasColumnName("SubmissionId").HasColumnType("bigint");
        builder.Property(x => x.PayrollYear).HasColumnName("PayrollYear").HasColumnType("nvarchar(10)");
        builder.Property(x => x.PayrollMonth).HasColumnName("PayrollMonth").HasColumnType("tinyint");
        builder.Property(x => x.CreatedDate).HasColumnName("CreatedDate").HasColumnType("datetime");
        builder.Property(x => x.EndOfYearAdjustment).HasColumnName("EndOfYearAdjustment").HasColumnType("bit");
        builder.Property(x => x.EndOfYearAdjustmentAmount).HasColumnName("EndOfYearAdjustmentAmount").HasColumnType("decimal(18, 4)");
        builder.Property(x => x.DateCeased).HasColumnName("DateCeased").HasColumnType("datetime");
        builder.Property(x => x.InactiveFrom).HasColumnName("InactiveFrom").HasColumnType("datetime");
        builder.Property(x => x.InactiveTo).HasColumnName("InactiveTo").HasColumnType("datetime");
        builder.Property(x => x.HmrcSubmissionId).HasColumnName("HmrcSubmissionId").HasColumnType("bigint");
        builder.Property(x => x.NoPaymentForPeriod).HasColumnName("NoPaymentForPeriod").HasColumnType("bit");
    }
}
