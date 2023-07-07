using AutoMapper;
using SFA.DAS.Employer.Shared.UI;
using SFA.DAS.Employer.Shared.UI.Attributes;
using SFA.DAS.EmployerFinance.Queries.GetTransactionsDownload;
using SFA.DAS.EmployerFinance.Queries.GetTransferTransactionDetails;
using SFA.DAS.EmployerFinance.Web.Authentication;
using SFA.DAS.EmployerFinance.Web.Helpers;
using SFA.DAS.EmployerFinance.Web.Infrastructure;
using SFA.DAS.EmployerFinance.Web.Orchestrators;
using SFA.DAS.EmployerFinance.Web.ViewModels;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerFinance.Web.Controllers
{
    [SetNavigationSection(NavigationSection.AccountsFinance)]
    [Route("accounts/{HashedAccountId}")]
    [Authorize(Policy = nameof(PolicyNames.HasEmployerViewerTransactorOwnerAccount))]
    public class EmployerAccountTransactionsController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IEncodingService _encodingService;

        private readonly IEmployerAccountTransactionsOrchestrator _accountTransactionsOrchestrator;

        public EmployerAccountTransactionsController(
            IEmployerAccountTransactionsOrchestrator accountTransactionsOrchestrator,
            IMapper mapper,
            IMediator mediator,
            IEncodingService encodingService)
        {
            _accountTransactionsOrchestrator = accountTransactionsOrchestrator;

            _mapper = mapper;
            _mediator = mediator;
            _encodingService = encodingService;
        }

        [Route("finance/provider/summary")]
        public async Task<IActionResult> ProviderPaymentSummary([FromRoute]string hashedAccountId, long ukprn, DateTime fromDate, DateTime toDate)
        {
            var viewModel = await _accountTransactionsOrchestrator.GetProviderPaymentSummary(hashedAccountId, ukprn, fromDate, toDate);

            return View(ControllerConstants.ProviderPaymentSummaryViewName, viewModel);
        }

        [Route("finance", Name = RouteNames.FinanceIndex)]
        public async Task<IActionResult> Index([FromRoute]string hashedAccountId)
        {

            var viewModel = await _accountTransactionsOrchestrator.Index(hashedAccountId, HttpContext.User.Identities.FirstOrDefault());

            if (viewModel.RedirectUrl != null)
                return Redirect(viewModel.RedirectUrl);

            return View(viewModel);
        }

        [Route("finance/downloadtransactions", Name = RouteNames.DownloadTransactionsGet)]
        public ActionResult TransactionsDownload()
        {
            return View(new TransactionDownloadViewModel());
        }

        [HttpPost]
        [Route("finance/downloadtransactions", Name = RouteNames.DownloadTransactionsPost)]
        public async Task<IActionResult> TransactionsDownload([FromRoute]string hashedAccountId, TransactionDownloadViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var response = await _mediator.Send(new GetTransactionsDownloadQuery
                {
                    AccountId = _encodingService.Decode(hashedAccountId,EncodingType.AccountId),
                    DownloadFormat = model.DownloadFormat,
                    EndDate = model.EndDate,
                    StartDate = model.StartDate
                });
                return File(response.FileData, response.MimeType, $"esfaTransactions_{DateTime.Now:yyyyMMddHHmmss}.{response.FileExtension}");
            }
            catch (ValidationException e)
            {
                foreach (var member in e.ValidationResult.MemberNames)
                {
                    ModelState.AddModelError(member.Split('|')[0], member.Split('|')[1]);
                }
                return View(model);
            }
        }

        [HttpGet]
        [Route("finance/{year}/{month}", Name = RouteNames.TransactionsView)]
        public async Task<IActionResult> TransactionsView([FromRoute]string hashedAccountId, [FromRoute]int year, [FromRoute]int month)
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
        public async Task<IActionResult> LevyDeclarationDetail([FromRoute]string hashedAccountId, DateTime fromDate, DateTime toDate)
        {
            var viewModel = await _accountTransactionsOrchestrator.FindAccountLevyDeclarationTransactions(hashedAccountId, fromDate, toDate);

            return View(ControllerConstants.LevyDeclarationDetailViewName, viewModel);
        }

        [Route("finance/course/standard/summary")]
        public async Task<IActionResult> CourseStandardPaymentSummary(string hashedAccountId, long ukprn, string courseName,
            int? courseLevel, DateTime fromDate, DateTime toDate)
        {
            return await CourseFrameworkPaymentSummary(hashedAccountId, ukprn, courseName, courseLevel, null, fromDate, toDate);
        }

        [Route("finance/course/framework/summary")]
        public async Task<IActionResult> CourseFrameworkPaymentSummary(string hashedAccountId, long ukprn, string courseName,
            int? courseLevel, int? pathwayCode, DateTime fromDate, DateTime toDate)
        {
            var orchestratorResponse = await _accountTransactionsOrchestrator.GetCoursePaymentSummary(
                hashedAccountId, ukprn, courseName, courseLevel, pathwayCode,
                fromDate, toDate);

            return View(ControllerConstants.CoursePaymentSummaryViewName, orchestratorResponse.Data);
        }

        [Route("finance/transfer/details")]
        public async Task<IActionResult> TransferDetail([FromRoute]string hashedAccountId, GetTransferTransactionDetailsQuery query)
        {
            query.AccountId = _encodingService.Decode(hashedAccountId, EncodingType.AccountId);
            var response = await _mediator.Send(query);

            var model = _mapper.Map<TransferTransactionDetailsViewModel>(response);
            return View(ControllerConstants.TransferDetailsViewName, model);
        }

    }
}