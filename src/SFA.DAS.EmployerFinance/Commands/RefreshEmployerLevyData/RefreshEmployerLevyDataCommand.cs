using System.Collections.Generic;
using MediatR;
using SFA.DAS.Authorization.ModelBinding;
using SFA.DAS.EmployerFinance.Models.HmrcLevy;

namespace SFA.DAS.EmployerFinance.Commands.RefreshEmployerLevyData
{
    public class RefreshEmployerLevyDataCommand : IAuthorizationContextModel,IRequest<Unit>
    {
        public long AccountId { get; set; }
        public ICollection<EmployerLevyData> EmployerLevyData { get; set; }
    }
}
