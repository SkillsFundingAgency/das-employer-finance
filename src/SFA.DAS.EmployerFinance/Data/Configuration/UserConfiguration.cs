using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.EmployerFinance.Models.UserProfile;

namespace SFA.DAS.EmployerFinance.Data.Configuration;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("User");
        builder.Ignore(u => u.FullName);
        builder.Ignore(u => u.UserRef);
        builder.Property(u => u.Ref).HasColumnName(nameof(User.UserRef));
    }
}