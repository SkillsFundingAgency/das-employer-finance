using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.EmployerFinance.Models.Payments;

namespace SFA.DAS.EmployerFinance.Data.Configuration;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payment");
        builder.Property(x => x.Id).HasColumnName("PaymentId");
        builder.Property(x => x.EmployerAccountId).HasColumnName("AccountId");
        builder.Ignore(a => a.StandardCode);
        builder.Ignore(a => a.FrameworkCode);
        builder.Ignore(a => a.ProgrammeType);
        builder.Ignore(a => a.PathwayCode);
        builder.Ignore(a => a.PathwayName);
    }
}