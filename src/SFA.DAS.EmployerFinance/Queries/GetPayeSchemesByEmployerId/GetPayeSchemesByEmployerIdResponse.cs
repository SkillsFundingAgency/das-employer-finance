using SFA.DAS.EmployerFinance.Models.Paye;

namespace SFA.DAS.EmployerFinance.Queries.GetPayeSchemesByEmployerId;

public class GetPayeSchemesByEmployerIdResponse
{
    public List<Paye> Schemes { get; set; }
}
