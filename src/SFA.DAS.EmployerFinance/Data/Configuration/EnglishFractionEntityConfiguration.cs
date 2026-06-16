using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.EmployerFinance.Models.Levy;

namespace SFA.DAS.EmployerFinance.Data.Configuration;

public class EnglishFractionEntityConfiguration : IEntityTypeConfiguration<EnglishFractionEntity>
{
    public void Configure(EntityTypeBuilder<EnglishFractionEntity> builder)
    {
        builder.ToTable("EnglishFraction");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("Id")
            .HasColumnType("bigint")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.DateCalculated)
            .HasColumnName("DateCalculated")
            .HasColumnType("datetime")
            .IsRequired();

        builder.Property(x => x.Amount)
            .HasColumnName("Amount")
            .HasColumnType("decimal(18, 5)");

        builder.Property(x => x.EmpRef)
            .HasColumnName("EmpRef")
            .HasColumnType("nvarchar")
            .HasMaxLength(50);

        builder.Property(x => x.DateCreated)
            .HasColumnName("DateCreated")
            .HasColumnType("datetime")
            .ValueGeneratedOnAdd();
    }
}
