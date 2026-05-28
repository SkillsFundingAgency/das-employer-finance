using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Models.Levy;

namespace SFA.DAS.EmployerFinance.Data;

public class EnglishFractionRepository : IEnglishFractionRepository
{
    private readonly Lazy<EmployerFinanceDbContext> _db;

    public EnglishFractionRepository(Lazy<EmployerFinanceDbContext> db)
    {
        _db = db;
    }

    public async Task CreateEmployerFraction(DasEnglishFraction fractions, string employerReference)
    {
        var entity = new EnglishFractionEntity
        {
            EmpRef = employerReference,
            Amount = fractions.Amount,
            DateCalculated = fractions.DateCalculated
        };

        await _db.Value.Set<EnglishFractionEntity>().AddAsync(entity);
        await _db.Value.SaveChangesAsync();
    }

    public async Task<IEnumerable<DasEnglishFraction>> GetAllEmployerFractions(string employerReference)
    {
        var entities = await _db.Value.Set<EnglishFractionEntity>()
            .Where(x => x.EmpRef == employerReference)
            .OrderByDescending(x => x.DateCalculated)
            .ToListAsync();

        return entities.Select(x => new DasEnglishFraction
        {
            Id = x.Id.ToString(),
            EmpRef = x.EmpRef,
            Amount = x.Amount ?? 0m,
            DateCalculated = x.DateCalculated
        });
    }

    public async Task<DateTime> GetLastUpdateDate()
    {
        return await _db.Value.Set<EnglishFractionCalculationDate>()
            .OrderByDescending(x => x.DateCalculated)
            .Select(x => x.DateCalculated)
            .FirstOrDefaultAsync();
    }

    public async Task<DateTime?> GetLastStoredCalculationDateForEmpRef(string employerReference)
    {
        return await _db.Value.Set<EnglishFractionEntity>()
            .Where(x => x.EmpRef == employerReference)
            .OrderByDescending(x => x.DateCalculated)
            .Select(x => (DateTime?)x.DateCalculated)
            .FirstOrDefaultAsync();
    }

    public async Task SetLastUpdateDate(DateTime dateUpdated)
    {
        await _db.Value.Set<EnglishFractionCalculationDate>().AddAsync(new EnglishFractionCalculationDate
        {
            DateCalculated = dateUpdated.Date
        });

        await _db.Value.SaveChangesAsync();
    }
}