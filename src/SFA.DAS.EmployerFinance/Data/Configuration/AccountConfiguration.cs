using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.EmployerFinance.Models.Account;

namespace SFA.DAS.EmployerFinance.Data.Configuration;

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("Account");
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id).HasColumnName("Id").HasColumnType("bigint").IsRequired().ValueGeneratedNever();
        builder.Property(x => x.Name).HasColumnName("Name").HasColumnType("varchar").HasMaxLength(100).IsRequired();
        
        builder.HasMany(a => a.ReceivedTransferConnectionInvitations);
        builder.HasMany(a => a.SentTransferConnectionInvitations);
        
        builder.Ignore(a => a.HashedId);
        builder.Ignore(a => a.PublicHashedId);
    }
}