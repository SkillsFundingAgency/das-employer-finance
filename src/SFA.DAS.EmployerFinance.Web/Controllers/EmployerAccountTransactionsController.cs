using System;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerFinance.Queries.GetTransferTransactionDetails;
using SFA.DAS.EmployerFinance.Web.Attributes;
using SFA.DAS.EmployerFinance.Web.Authentication;
using SFA.DAS.EmployerFinance.Web.Helpers;
using SFA.DAS.EmployerFinance.Web.Orchestrators;
using SFA.DAS.EmployerFinance.Web.ViewModels;

namespace SFA.DAS.EmployerFinance.Web.Controllers
{
    [Route("accounts/{HashedAccountId}")]
    [Authorize(Policy = nameof(PolicyNames.HasEmployerViewerTransactorOwnerAccount))]
    public class EmployerAccountTransactionsController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly ILogger<EmployerAccountTransactionsController> _logger;

        private readonly EmployerAccountTransactionsOrchestrator _accountTransactionsOrchestrator;

        public EmployerAccountTransactionsController(
            EmployerAccountTransactionsOrchestrator accountTransactionsOrchestrator,
            IMapper mapper,
            IMediator mediator,
            ILogger<EmployerAccountTransactionsController> logger)
        {
            _accountTransactionsOrchestrator = accountTransactionsOrchestrator;

            _mapper = mapper;
            _mediator = mediator;
            _logger = logger;
        }

        [Route("finance/provider/summary")]
        [Route("balance/provider/summary")]
        public async Task<IActionResult> ProviderPaymentSummary([FromRoute]string hashedAccountId, long ukprn, DateTime fromDate, DateTime toDate)
        {
            var viewModel = await _accountTransactionsOrchestrator.GetProviderPaymentSummary(hashedAccountId, ukprn, fromDate, toDate);

            return View(ControllerConstants.ProviderPaymentSummaryViewName, viewModel);
        }

        [Route("finance")]
        [Route("balance")]
        public async Task<IActionResult> Index([FromRoute]string hashedAccountId)
        {

            var viewModel = await _accountTransactionsOrchestrator.Index(hashedAccountId);

            if (viewModel.RedirectUrl != null)
                return Redirect(viewModel.RedirectUrl);

            return View(viewModel);
        }

        [ImportModelStateFromTempData]
        [Route("finance/downloadtransactions")]
        public ActionResult TransactionsDownload()
        {
            return View(new TransactionDownloadViewModel());
        }

        [HttpPost]
        [ValidateModelState]
        [Route("finance/downloadtransactions")]
        public async Task<IActionResult> TransactionsDownload(TransactionDownloadViewModel model)
        {
            var response = await _mediator.Send(model.GetTransactionsDownloadQuery);
            return File(response.FileData, response.MimeType, $"esfaTransactions_{DateTime.Now:yyyyMMddHHmmss}.{response.FileExtension}");
        }

        [Route("finance/{year}/{month}")]
        [Route("balance/{year}/{month}")]
        public async Task<IActionResult> TransactionsView(string hashedAccountId, int year, int month)
        {
            var transactionViewResult = await _accountTransactionsOrchestrator.GetAccountTransactions(hashedAccountId, year, month);

            if (transactionViewResult.Data.Account == null)
            {
                return RedirectToAction(ControllerConstants.IndexActionName, ControllerConstants.AccessDeniedControllerName, 
                    new { hashedAccountId = hashedAccountId});
            }

            transactionViewResult.Data.Model.Data.HashedAccountId = hashedAccountId;

            return View(transactionViewResult);
        }


        [Route("finance/levyDeclaration/details")]
        [Route("balance/levyDeclaration/details")]
        public async Task<IActionResult> LevyDeclarationDetail(string hashedAccountId, DateTime fromDate, DateTime toDate)
        {
            var viewModel = await _accountTransactionsOrchestrator.FindAccountLevyDeclarationTransactions(hashedAccountId, fromDate, toDate);

            return View(ControllerConstants.LevyDeclarationDetailViewName, viewModel);
        }

        [Route("finance/course/standard/summary")]
        [Route("balance/course/standard/summary")]
        public async Task<IActionResult> CourseStandardPaymentSummary(string hashedAccountId, long ukprn, string courseName,
            int? courseLevel, DateTime fromDate, DateTime toDate)
        {
            return await CourseFrameworkPaymentSummary(hashedAccountId, ukprn, courseName, courseLevel, null, fromDate, toDate);
        }

        [Route("finance/course/framework/summary")]
        [Route("balance/course/framework/summary")]
        public async Task<IActionResult> CourseFrameworkPaymentSummary(string hashedAccountId, long ukprn, string courseName,
            int? courseLevel, int? pathwayCode, DateTime fromDate, DateTime toDate)
        {
            var orchestratorResponse = await _accountTransactionsOrchestrator.GetCoursePaymentSummary(
                hashedAccountId, ukprn, courseName, courseLevel, pathwayCode,
                fromDate, toDate);

            return View(ControllerConstants.CoursePaymentSummaryViewName, orchestratorResponse.Data);
        }

        [Route("finance/transfer/details")]
        [Route("balance/transfer/details")]
        public async Task<IActionResult> TransferDetail(GetTransferTransactionDetailsQuery query)
        {
            var response = await _mediator.Send(query);

            var model = _mapper.Map<TransferTransactionDetailsViewModel>(response);
            return View(ControllerConstants.TransferDetailsViewName, model);
        }

    }
}