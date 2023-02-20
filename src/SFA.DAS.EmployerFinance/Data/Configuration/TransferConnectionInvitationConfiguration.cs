using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.EmployerFinance.Models.TransferConnections;

namespace SFA.DAS.EmployerFinance.Data.Configuration;

public class TransferConnectionInvitationConfiguration : IEntityTypeConfiguration<TransferConnectionInvitation>
{
    public void Configure(EntityTypeBuilder<TransferConnectionInvitation> builder)
    {
       builder
           .Property(i => i.ReceiverAccount)
           .IsRequired();

       builder
           .Property(i => i.SenderAccount)
           .IsRequired();
    }
}