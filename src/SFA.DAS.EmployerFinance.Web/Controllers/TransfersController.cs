﻿using System.Threading.Tasks;
using System.Web.Mvc;
using SFA.DAS.Authorization.Mvc.Attributes;
using SFA.DAS.EmployerFinance.Web.Orchestrators;

namespace SFA.DAS.EmployerFinance.Web.Controllers
{
    [DasAuthorize("EmployerFeature.TransfersMatching")]
    [RoutePrefix("accounts/{HashedAccountId}")] public class TransfersController : Controller
    {
        private readonly TransfersOrchestrator _transfersOrchestrator;

        public TransfersController(TransfersOrchestrator transfersOrchestrator)
        {
            _transfersOrchestrator = transfersOrchestrator;
        }

        [HttpGet]
        [Route("transfers")]
        public async Task<ActionResult> Index(string hashedAccountId)
        {
            var viewModel = await _transfersOrchestrator.GetIndexViewModel(hashedAccountId);

            return View(viewModel);
        }
    }
}