using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.EmployerFinance.Models;

namespace SFA.DAS.EmployerFinance.Data.Configuration;

public class HealthCheckConfiguration: IEntityTypeConfiguration<HealthCheck>
{
    public void Configure(EntityTypeBuilder<HealthCheck> builder)
    {
        builder.ToTable("HealthChecks", "dbo");
    }
}