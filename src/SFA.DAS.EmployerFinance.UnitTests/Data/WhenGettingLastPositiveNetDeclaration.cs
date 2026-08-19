using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Data;
using SFA.DAS.EmployerFinance.Interfaces;
using SFA.DAS.EmployerFinance.Models.Levy;

namespace SFA.DAS.EmployerFinance.UnitTests.Data;

[TestFixture]
[Parallelizable]
public class WhenGettingLastPositiveNetDeclaration
{
    [Test]
    public async Task ThenSelectsDeclarationWithPositiveLevyDueEvenWhenAllowanceIsNull()
    {
        await using var fixture = new Fixture();
        fixture.AddDeclaration(accountId: 1, levyDueYtd: 100, levyAllowanceForYear: null, submissionDate: new DateTime(2024, 1, 1));

        var result = await fixture.Repository.GetLastPositiveNetDeclarationForAccount(1);

        result.Should().NotBeNull();
        result.SubmissionDate.Should().Be(new DateTime(2024, 1, 1));
    }

    [Test]
    public async Task ThenSelectsDeclarationWhenLevyDueIsPositiveEvenIfNetAfterAllowanceIsNot()
    {
        await using var fixture = new Fixture();
        fixture.AddDeclaration(accountId: 1, levyDueYtd: 50, levyAllowanceForYear: 100, submissionDate: new DateTime(2024, 2, 1));

        var result = await fixture.Repository.GetLastPositiveNetDeclarationForAccount(1);

        result.Should().NotBeNull();
        result.SubmissionDate.Should().Be(new DateTime(2024, 2, 1));
    }

    [Test]
    public async Task ThenIgnoresDeclarationsWithZeroOrNullLevyDue()
    {
        await using var fixture = new Fixture();
        fixture.AddDeclaration(accountId: 1, levyDueYtd: 0, levyAllowanceForYear: 0, submissionDate: new DateTime(2024, 3, 1));
        fixture.AddDeclaration(accountId: 1, levyDueYtd: null, levyAllowanceForYear: 0, submissionDate: new DateTime(2024, 4, 1));

        var result = await fixture.Repository.GetLastPositiveNetDeclarationForAccount(1);

        result.Should().BeNull();
    }

    [Test]
    public async Task ThenReturnsLatestSubmissionDate()
    {
        await using var fixture = new Fixture();
        fixture.AddDeclaration(accountId: 1, levyDueYtd: 10, levyAllowanceForYear: null, submissionDate: new DateTime(2024, 1, 1));
        fixture.AddDeclaration(accountId: 1, levyDueYtd: 20, levyAllowanceForYear: null, submissionDate: new DateTime(2024, 6, 1));

        var result = await fixture.Repository.GetLastPositiveNetDeclarationForAccount(1);

        result.SubmissionDate.Should().Be(new DateTime(2024, 6, 1));
    }

    [Test]
    public async Task ThenMapsNullBooleanFlagsToFalse()
    {
        await using var fixture = new Fixture();
        fixture.AddDeclaration(
            accountId: 1,
            levyDueYtd: 100,
            levyAllowanceForYear: null,
            submissionDate: new DateTime(2024, 7, 1),
            endOfYearAdjustment: null,
            noPaymentForPeriod: null);

        var result = await fixture.Repository.GetLastPositiveNetDeclarationForAccount(1);

        result.Should().NotBeNull();
        result.SubmissionDate.Should().Be(new DateTime(2024, 7, 1));
        result.EndOfYearAdjustment.Should().BeFalse();
        result.NoPaymentForPeriod.Should().BeFalse();
    }

    private sealed class Fixture : IAsyncDisposable
    {
        private readonly EmployerFinanceDbContext _db;

        public DasLevyRepository Repository { get; }

        public Fixture()
        {
            _db = new EmployerFinanceDbContext(new DbContextOptionsBuilder<EmployerFinanceDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);

            Repository = new DasLevyRepository(
                new EmployerFinanceConfiguration(),
                new Lazy<EmployerFinanceDbContext>(() => _db),
                Mock.Of<ICurrentDateTime>());
        }

        public void AddDeclaration(
            long accountId,
            decimal? levyDueYtd,
            decimal? levyAllowanceForYear,
            DateTime submissionDate,
            bool? endOfYearAdjustment = false,
            bool? noPaymentForPeriod = false)
        {
            _db.LevyDeclarations.Add(new LevyDeclarationEntity
            {
                AccountId = accountId,
                EmpRef = $"{accountId}/REF",
                LevyDueYtd = levyDueYtd,
                LevyAllowanceForYear = levyAllowanceForYear,
                SubmissionDate = submissionDate,
                SubmissionId = submissionDate.Ticks,
                PayrollYear = "24-25",
                PayrollMonth = 1,
                CreatedDate = submissionDate,
                EndOfYearAdjustment = endOfYearAdjustment,
                NoPaymentForPeriod = noPaymentForPeriod
            });
            _db.SaveChanges();
        }

        public ValueTask DisposeAsync() => _db.DisposeAsync();
    }
}
