using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.EmployerFinance.Models.Account;
using SFA.DAS.EmployerFinance.Models.TransferConnections;

namespace SFA.DAS.EmployerFinance.Data.Configuration;

public class TransferConnectionInvitationConfiguration : IEntityTypeConfiguration<TransferConnectionInvitation>
{
    public void Configure(EntityTypeBuilder<TransferConnectionInvitation> builder)
    {
        builder.ToTable("TransferConnectionInvitation");
        builder.HasKey(x=>x.Id);
        builder.Property(x => x.SenderAccountId).HasColumnName("SenderAccountId").HasColumnType("bigint").IsRequired();
        builder.Property(x => x.ReceiverAccountId).HasColumnName("ReceiverAccountId").HasColumnType("bigint").IsRequired();
        builder.Property(x => x.Status).HasColumnName("Status").HasColumnType("int");
        builder.Property(x => x.DeletedBySender).HasColumnName("DeletedBySender").HasColumnType("bit");
        builder.Property(x => x.DeletedByReceiver).HasColumnName("DeletedByReceiver").HasColumnType("bit");
        builder.Property(x => x.CreatedDate).HasColumnName("CreatedDate").HasColumnType("datetime");
        builder.HasOne(x=> x.ReceiverAccount)
            .WithMany(c=>c.ReceivedTransferConnectionInvitations)
           .HasForeignKey(k => k.ReceiverAccountId).HasPrincipalKey(c=>c.Id);
        builder.HasOne(x=> x.SenderAccount)
            .WithMany(c=>c.SentTransferConnectionInvitations)
            .HasForeignKey(k => k.SenderAccountId).HasPrincipalKey(c=>c.Id);
       
        
    }
}

public class TransferConnectionInvitationChangeConfiguration : IEntityTypeConfiguration<TransferConnectionInvitationChange>
{
    public void Configure(EntityTypeBuilder<TransferConnectionInvitationChange> builder)
    {
        builder.ToTable("TransferConnectionInvitationChange");
        builder.HasKey(x=>x.Id);
        builder.Property(x => x.TransferConnectionInvitationId).HasColumnName("TransferConnectionInvitationId").HasColumnType("int");
        builder.Property(x => x.SenderAccountId).HasColumnName("SenderAccountId").HasColumnType("bigint").IsRequired();
        builder.Property(x => x.ReceiverAccountId).HasColumnName("ReceiverAccountId").HasColumnType("bigint").IsRequired();
        builder.Property(x => x.Status).HasColumnName("Status").HasColumnType("int");
        builder.Property(x => x.DeletedBySender).HasColumnName("DeletedBySender").HasColumnType("bit");
        builder.Property(x => x.DeletedByReceiver).HasColumnName("DeletedByReceiver").HasColumnType("bit");
        builder.Property(x => x.CreatedDate).HasColumnName("CreatedDate").HasColumnType("datetime");
        builder.Property(x => x.UserId).HasColumnName("UserId").HasColumnType("bigint");
        builder.HasOne(x=> x.ReceiverAccount)
            .WithMany()
            .HasForeignKey(k => k.ReceiverAccountId);

        builder.HasOne(x => x.SenderAccount)
            .WithMany()
            .HasForeignKey(k => k.SenderAccountId);
    }
}