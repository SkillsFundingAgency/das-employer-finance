namespace SFA.DAS.EmployerFinance.Web.Controllers;

[Route("[Controller]")]
public class DocumentationController: Controller
{
    [Route("reports/transactions/v2")]
    public IActionResult TransactionsReportV2()
    {
        return View();
    }
}