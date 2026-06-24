using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.EmployerFinance.Models.Levy;

namespace SFA.DAS.EmployerFinance.Data.Configuration;

public class LevyDeclarationConfiguration : IEntityTypeConfiguration<LevyDeclaration>
{
    public void Configure(EntityTypeBuilder<LevyDeclaration> builder)
    {
        builder.ToTable("LevyDeclaration");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
               .HasColumnName("Id");

        builder.Property(x => x.AccountId)
               .HasColumnName("AccountId");

        builder.Property(x => x.EmpRef)
               .HasMaxLength(50)
               .IsRequired();

        builder.Property(x => x.LevyDueYTD)
               .HasPrecision(18, 4);

        builder.Property(x => x.LevyAllowanceForYear)
               .HasPrecision(18, 4);

        builder.Property(x => x.EndOfYearAdjustmentAmount)
               .HasPrecision(18, 4);

        builder.Property(x => x.PayrollYear)
               .HasMaxLength(10);

        builder.Property(x => x.CreatedDate);

        builder.Property(x => x.EndOfYearAdjustment);

        builder.Property(x => x.NoPaymentForPeriod);
    }
}
