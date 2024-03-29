using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.EmployerFinance.Models.Transfers;

namespace SFA.DAS.EmployerFinance.Data.Configuration;

public class AccountTransferConfiguration : IEntityTypeConfiguration<AccountTransfer>
{
    public void Configure(EntityTypeBuilder<AccountTransfer> builder)
    {
        builder.ToTable("AccountTransfers");
        builder.HasKey(x => x.Id);
        builder.Ignore(c => c.ApprenticeCount);
    }
}