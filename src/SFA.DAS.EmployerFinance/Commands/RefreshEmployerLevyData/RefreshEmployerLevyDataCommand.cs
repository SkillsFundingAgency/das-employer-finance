using SFA.DAS.EmployerFinance.Models.HmrcLevy;

namespace SFA.DAS.EmployerFinance.Commands.RefreshEmployerLevyData;

public class RefreshEmployerLevyDataCommand : IRequest<Unit>
{
    public long AccountId { get; set; }
    public ICollection<EmployerLevyData> EmployerLevyData { get; set; }
}