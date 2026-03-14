using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.EmployerFinance.Models.Paye;

namespace SFA.DAS.EmployerFinance.Data.Configuration;

public class PayeConfiguration : IEntityTypeConfiguration<Paye>
{
    public void Configure(EntityTypeBuilder<Paye> builder)
    {
        builder.ToTable("AccountPaye");
        builder.HasKey(p => new { p.AccountId, p.EmpRef });

        builder.Property(p => p.AccountId).HasColumnName("AccountId").HasColumnType("bigint").IsRequired();
        builder.Property(p => p.EmpRef).HasColumnName("EmpRef").HasColumnType("nvarchar").HasMaxLength(16);
        builder.Property(p => p.Name).HasColumnName("Name").HasColumnType("varchar").HasMaxLength(500);
        builder.Property(p => p.Aorn).HasColumnName("Aorn").HasColumnType("varchar").HasMaxLength(25);
    }
}
