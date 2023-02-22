using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.EmployerFinance.Models.Account;

namespace SFA.DAS.EmployerFinance.Data.Configuration;

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("Account");
        builder.HasMany(a => a.ReceivedTransferConnectionInvitations);
        builder.HasMany(a => a.SentTransferConnectionInvitations);
        
        builder.Ignore(a => a.HashedId);
        builder.Ignore(a => a.PublicHashedId);
    }
}