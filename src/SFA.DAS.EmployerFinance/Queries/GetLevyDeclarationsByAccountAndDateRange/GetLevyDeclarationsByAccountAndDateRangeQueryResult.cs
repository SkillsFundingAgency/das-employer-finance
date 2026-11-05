using SFA.DAS.EmployerFinance.Models.Levy;

namespace SFA.DAS.EmployerFinance.Queries.GetLevyDeclarationsByAccountAndDateRange;

public sealed record GetLevyDeclarationsByAccountAndDateRangeQueryResult(List<LevyDeclarationItem> _);