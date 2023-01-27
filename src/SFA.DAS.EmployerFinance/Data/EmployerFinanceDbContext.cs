using SFA.DAS.EmployerFinance.Models;
using SFA.DAS.EmployerFinance.Models.Payments;
using SFA.DAS.EmployerFinance.Models.Transaction;
using SFA.DAS.EntityFramework;
using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.EmployerFinance.Models.Account;
using SFA.DAS.EmployerFinance.Models.TransferConnections;
using SFA.DAS.EmployerFinance.Models.UserProfile;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Core.Objects;
using SFA.DAS.HashingService;
using SFA.DAS.EmployerFinance.MarkerInterfaces;
using Microsoft.Azure.Services.AppAuthentication;
using SFA.DAS.EmployerFinance.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Data.SqlClient;
using System;

namespace SFA.DAS.EmployerFinance.Data
{
    public class EmployerFinanceDbContext : DbContext
    {
        private readonly EmployerFinanceConfiguration _configuration;
        private readonly AzureServiceTokenProvider _azureServiceTokenProvider;

        private const string AzureResource = "https://database.windows.net/";

        public virtual DbSet<Account> Accounts { get; set; }
        public virtual DbSet<HealthCheck> HealthChecks { get; set; }
        public virtual DbSet<PeriodEnd> PeriodEnds { get; set; }
        public virtual DbSet<Payment> Payments { get; set; }
        public virtual DbSet<TransactionLineEntity> Transactions { get; set; }
        public virtual DbSet<TransferConnectionInvitation> TransferConnectionInvitations { get; set; }
        public virtual DbSet<User> Users { get; set; }

       public EmployerFinanceDbContext(DbContextOptions options) : base(options) { }

        public EmployerFinanceDbContext(IOptions<EmployerFinanceConfiguration> configuaration, DbContextOptions options, AzureServiceTokenProvider azureServiceTokenProvider)
        {
            _configuration=configuaration.Value;
            _azureServiceTokenProvider = azureServiceTokenProvider;
        }        

        protected EmployerFinanceDbContext()
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseLazyLoadingProxies();

            if (_configuration == null || _azureServiceTokenProvider == null)
            {
                optionsBuilder.UseSqlServer().UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
                return;
            }

            var connection = new SqlConnection
            {
                ConnectionString = _configuration.DatabaseConnectionString,
                AccessToken = _azureServiceTokenProvider.GetAccessTokenAsync(AzureResource).Result,
            };

            optionsBuilder.UseSqlServer(connection, options =>
                options.EnableRetryOnFailure(
                    5,
                    TimeSpan.FromSeconds(20),
                    null
                )).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("employer_financial");
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(EmployerFinanceDbContext).Assembly);
        }
    }
}