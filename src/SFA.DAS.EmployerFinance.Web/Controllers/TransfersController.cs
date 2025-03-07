using SFA.DAS.Employer.Shared.UI;
using SFA.DAS.Employer.Shared.UI.Attributes;
using SFA.DAS.EmployerFinance.Web.Authentication;
using SFA.DAS.EmployerFinance.Web.Orchestrators;

namespace SFA.DAS.EmployerFinance.Web.Controllers;

[SetNavigationSection(NavigationSection.AccountsFinance)]
[Authorize(Policy = nameof(PolicyNames.HasEmployerViewerTransactorOwnerAccount))]
[Route("accounts/{HashedAccountId}")]
public class TransfersController(
    TransfersOrchestrator transfersOrchestrator) : Controller
{
    [HttpGet]
    [Route("transfers")]
    public async Task<IActionResult> Index(string hashedAccountId)
    {
        var viewModel = await transfersOrchestrator.GetIndexViewModel(hashedAccountId);

        return View(viewModel);
    }

    [HttpGet]
    [Route("transfers/financial-breakdown")]
    public IActionResult FinancialBreakdown()
    {
        return RedirectToAction("Index", "Error", new { statusCode = 404 });
    }
}