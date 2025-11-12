using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.EmployerFinance.Models.Payments;

namespace SFA.DAS.EmployerFinance.Data.Configuration;

public class PeriodEndConfiguration : IEntityTypeConfiguration<PeriodEnd>
{
    public void Configure(EntityTypeBuilder<PeriodEnd> builder)
    {
        builder.ToTable("PeriodEnd");
        builder.HasKey(x => x.Id);
    }
}
