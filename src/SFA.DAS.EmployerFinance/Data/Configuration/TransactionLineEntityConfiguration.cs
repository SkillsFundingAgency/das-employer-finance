using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.EmployerFinance.Models.Transaction;

namespace SFA.DAS.EmployerFinance.Data.Configuration;

public class TransactionLineEntityConfiguration : IEntityTypeConfiguration<TransactionLineEntity>
{
    public void Configure(EntityTypeBuilder<TransactionLineEntity> builder)
    {
        builder.ToTable("TransactionLine");
        builder.HasKey(t => t.Id);
    }
}