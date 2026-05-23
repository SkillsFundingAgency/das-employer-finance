using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.EmployerFinance.Models.Levy;

namespace SFA.DAS.EmployerFinance.Data.Configuration;

public class LevyDeclarationAndTopUpViewConfiguration : IEntityTypeConfiguration<LevyDeclarationAndTopUpView>
{
    public void Configure(EntityTypeBuilder<LevyDeclarationAndTopUpView> builder)
    {
        builder.HasNoKey();
        builder.ToView("GetLevyDeclarationAndTopUp");
    }
}
