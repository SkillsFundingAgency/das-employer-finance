using AutoMapper;
using SFA.DAS.EmployerFinance.Api.Types;

namespace SFA.DAS.EmployerFinance.Api.Mappings;

public class PeriodEndMappings : Profile
{
    public PeriodEndMappings()
    {
        CreateMap<Models.Payments.PeriodEnd, PeriodEnd>();

        CreateMap<PeriodEnd, Models.Payments.PeriodEnd>();
    }
}
