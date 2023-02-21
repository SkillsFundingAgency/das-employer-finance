using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.EmployerFinance.Models.TransferConnections;

namespace SFA.DAS.EmployerFinance.Data.Configuration;

public class TransferConnectionInvitationConfiguration : IEntityTypeConfiguration<TransferConnectionInvitation>
{
    public void Configure(EntityTypeBuilder<TransferConnectionInvitation> builder)
    {
       builder.HasOne(x=> x.ReceiverAccount)
           .WithMany()
           .HasForeignKey(k => k.ReceiverAccountId);

        builder.HasOne(x => x.SenderAccount)
            .WithMany()
            .HasForeignKey(k => k.SenderAccountId);
    }
}