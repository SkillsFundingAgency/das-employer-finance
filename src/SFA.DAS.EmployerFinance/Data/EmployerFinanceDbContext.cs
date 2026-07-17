using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Data.Configuration;
using SFA.DAS.EmployerFinance.Models;
using SFA.DAS.EmployerFinance.Models.Account;
using SFA.DAS.EmployerFinance.Models.AuditLogs;
using SFA.DAS.EmployerFinance.Models.Levy;
using SFA.DAS.EmployerFinance.Models.Paye;
using SFA.DAS.EmployerFinance.Models.Payments;
using SFA.DAS.EmployerFinance.Models.PaymentStaging;
using SFA.DAS.EmployerFinance.Models.Transaction;
using SFA.DAS.EmployerFinance.Models.TransferConnections;
using SFA.DAS.EmployerFinance.Models.Transfers;
using SFA.DAS.EmployerFinance.Models.UserProfile;


namespace SFA.DAS.EmployerFinance.Data;

[ExcludeFromCodeCoverage]
public class EmployerFinanceDbContext : DbContext
{
    private readonly EmployerFinanceConfiguration _configuration;
    private readonly IDbConnection _connection;
    public virtual DbSet<Account> Accounts { get; set; }
    public virtual DbSet<AccountTransfer> AccountTransfers { get; set; }
    public virtual DbSet<HealthCheck> HealthChecks { get; set; }
    public virtual DbSet<PeriodEnd> PeriodEnds { get; set; }
    public virtual DbSet<Payment> Payments { get; set; }
    public virtual DbSet<Paye> AccountPayes { get; set; }
    public virtual DbSet<EnglishFractionEntity> EnglishFractions { get; set; }
    public virtual DbSet<EnglishFractionCalculationDate> EnglishFractionCalculationDates { get; set; }
    public virtual DbSet<TransactionLineEntity> Transactions { get; set; }
    public virtual DbSet<TransferConnectionInvitation> TransferConnectionInvitations { get; set; }
    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<PaymentMetaData> PaymentMetaData{ get; set; }
    public virtual DbSet<TransferStaging> TransferStaging { get; set; }
    public virtual DbSet<PaymentStagingModel> PaymentStaging { get; set; }
    public virtual DbSet<PaymentMetaDataStaging> PaymentMetaDataStaging{ get; set; }
    public virtual DbSet<JobRun> JobRuns { get; set; }
    public virtual DbSet<WorkflowLog> WorkflowLogs { get; set; }
    public virtual DbSet<LevyDeclarationEntity> LevyDeclarations { get; set; }

    public EmployerFinanceDbContext() { }   

    public EmployerFinanceDbContext(DbContextOptions options) : base(options) { }

    public EmployerFinanceDbContext(IDbConnection connection, EmployerFinanceConfiguration configuration, DbContextOptions options) : base(options)
    {
        _configuration = configuration;
        _connection = connection;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (_configuration == null)
        {
            optionsBuilder.UseSqlServer().UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            return;
        }

        optionsBuilder.UseSqlServer(_connection as DbConnection);

    }
    public virtual Task<List<T>> SqlQueryAsync<T>(string query, params object[] parameters)
    {
        return Database.SqlQueryRaw<T>(query, parameters).ToListAsync();
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("employer_financial");
        modelBuilder.ApplyConfiguration(new AccountConfiguration());
        modelBuilder.ApplyConfiguration(new AccountTransferConfiguration());
        modelBuilder.ApplyConfiguration(new HealthCheckConfiguration());
        modelBuilder.ApplyConfiguration(new PayeConfiguration());
        modelBuilder.ApplyConfiguration(new EnglishFractionEntityConfiguration());
        modelBuilder.ApplyConfiguration(new EnglishFractionCalculationDateConfiguration());
        modelBuilder.ApplyConfiguration(new JobRunConfiguration());
        modelBuilder.ApplyConfiguration(new PaymentConfiguration());
        modelBuilder.ApplyConfiguration(new PaymentMetadataConfiguration());
        modelBuilder.ApplyConfiguration(new PaymentMetadataStagingConfiguration());
        modelBuilder.ApplyConfiguration(new TransactionLineEntityConfiguration());
        modelBuilder.ApplyConfiguration(new TransferConnectionInvitationConfiguration());
        modelBuilder.ApplyConfiguration(new TransferConnectionInvitationChangeConfiguration());
        modelBuilder.ApplyConfiguration(new PeriodEndConfiguration());
        modelBuilder.ApplyConfiguration(new PaymentStagingConfiguration());
        modelBuilder.ApplyConfiguration(new TransferStagingConfiguration());
        modelBuilder.ApplyConfiguration(new UserConfiguration());//Maybe delete this table
        modelBuilder.ApplyConfiguration(new WorkflowLogConfiguration());
        modelBuilder.ApplyConfiguration(new LevyDeclarationEntityConfiguration());
        modelBuilder.Ignore<PaymentDetails>();
    }
}
