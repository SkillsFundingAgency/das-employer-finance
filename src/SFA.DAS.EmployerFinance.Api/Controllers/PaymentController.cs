using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerFinance.Api.Controllers;

[Route("api/payments")]
public class PaymentController : Controller
{
    [HttpPut("{paymentId}/metadata/staging")]
    public async Task<IActionResult> PaymentMetadata(string paymentId, [FromBody] Payment request)
    {
        return View();
    }
}
