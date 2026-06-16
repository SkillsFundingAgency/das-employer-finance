using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.EmployerFinance.Models.Levy;

namespace SFA.DAS.EmployerFinance.Data.Configuration;

public class EnglishFractionCalculationDateConfiguration : IEntityTypeConfiguration<EnglishFractionCalculationDate>
{
    public void Configure(EntityTypeBuilder<EnglishFractionCalculationDate> builder)
    {
        builder.ToTable("EnglishFractionCalculationDate");
        builder.HasKey(x => x.DateCalculated);

        builder.Property(x => x.DateCalculated)
            .HasColumnName("DateCalculated")
            .HasColumnType("date")
            .IsRequired();
    }
}
