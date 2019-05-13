﻿using System;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using SFA.DAS.Authentication;
using SFA.DAS.Authorization;
using SFA.DAS.EmployerAccounts.Interfaces;
using SFA.DAS.EmployerAccounts.Web.Helpers;
using SFA.DAS.EmployerAccounts.Web.Orchestrators;
using SFA.DAS.EmployerAccounts.Web.ViewModels;
using SFA.DAS.Validation;
using System.Linq;
using SFA.DAS.EAS.Portal.Client;
using SFA.DAS.HashingService;
using Account = SFA.DAS.EAS.Portal.Types.Account;

namespace SFA.DAS.EmployerAccounts.Web.Controllers
{
    [Authorize]
    [RoutePrefix("accounts/{HashedAccountId}/teams")]
    public class EmployerTeamController : BaseController
    {
        private readonly EmployerTeamOrchestrator _employerTeamOrchestrator;
        private readonly IPortalClient _portalClient;
        private readonly IHashingService _hashingService;
        private static EmulatedFundingViewModel _emulatedFundingViewModel;

        public EmployerTeamController(
            IAuthenticationService owinWrapper)
            : base(owinWrapper)
        {
            _employerTeamOrchestrator = null;
        }

        public EmployerTeamController(
            IAuthenticationService owinWrapper,
            IAuthorizationService authorization,
            IMultiVariantTestingService multiVariantTestingService,
            ICookieStorageService<FlashMessageViewModel> flashMessage,
            EmployerTeamOrchestrator employerTeamOrchestrator,
            IPortalClient portalClient,
            IHashingService hashingService)
            : base(owinWrapper, multiVariantTestingService, flashMessage)
        {
            _employerTeamOrchestrator = employerTeamOrchestrator;
            _portalClient = portalClient;
            _hashingService = hashingService;
        }

        [HttpGet]
        [Route]
        public async Task<ActionResult> Index(string hashedAccountId)
        {
            if (FeatureToggles.Features.HomePage.Enabled)
            {
                var response = new OrchestratorResponse<NewHomepageViewModel> { Data = new NewHomepageViewModel()};
                var unhashedAccountId = _hashingService.DecodeValue(hashedAccountId);
                var result = await _portalClient.GetAccount(1337);
                if (result == null)
                {
                    NewHomepageHelper homepageHelper = new NewHomepageHelper(_hashingService);
                    OrchestratorResponse<AccountDashboardViewModel> oldAccountInfo = await GetAccountInformation(hashedAccountId);
                    var newAccountInfo = homepageHelper.ConvertFromOldModelToNewModel(oldAccountInfo);
                    response.Data.Account = newAccountInfo;

                }
                else
                {
                    var tempAcc = new Account();
                    tempAcc.Id = _hashingService.HashValue(result.AccountId);
                    //tempAcc.Organisations.Add(result.AccountLegalEntities.Select());

                    response.Data.Account = tempAcc;
                }
                response.Data.Flags.AgreementsToSign = response.Data.Account.Organisations.FirstOrDefault().Agreements.Select(x => x.IsPending).Count() > 0;
                response.Data.EmulatedFundingViewModel = _emulatedFundingViewModel;
                return View(response);
            }
            else
            {
                OrchestratorResponse<AccountDashboardViewModel> response = await GetAccountInformation(hashedAccountId);
                response.Data.EmulatedFundingViewModel = _emulatedFundingViewModel;
                return View(response);
            }
            
        }

        [HttpGet]
        [Route("view")]
        public async Task<ActionResult> ViewTeam(string hashedAccountId)
        {

            var response = await _employerTeamOrchestrator.GetTeamMembers(hashedAccountId, OwinWrapper.GetClaimValue(ControllerConstants.UserRefClaimKeyName));

            var flashMessage = GetFlashMessageViewModelFromCookie();
            if (flashMessage != null)
            {
                response.FlashMessage = flashMessage;
            }

            return View(response);
        }

        [HttpGet]
        [Route("invite")]
        public async Task<ActionResult> Invite(string hashedAccountId)
        {
            var response = await _employerTeamOrchestrator.GetNewInvitation(hashedAccountId, OwinWrapper.GetClaimValue(ControllerConstants.UserRefClaimKeyName));

            return View(response);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("invite")]
        public async Task<ActionResult> Invite(InviteTeamMemberViewModel model)
        {
            var response = await _employerTeamOrchestrator.InviteTeamMember(model, OwinWrapper.GetClaimValue(ControllerConstants.UserRefClaimKeyName));

            if (response.Status == HttpStatusCode.OK)
            {
                var flashMessage = new FlashMessageViewModel
                {
                    HiddenFlashMessageInformation = "page-invite-team-member-sent",
                    Severity = FlashMessageSeverityLevel.Success,
                    Headline = "Invitation sent",
                    Message = $"You've sent an invitation to <strong>{model.Email}</strong>"
                };
                AddFlashMessageToCookie(flashMessage);

                return RedirectToAction(ControllerConstants.NextStepsActionName);
            }


            model.ErrorDictionary = response.FlashMessage.ErrorMessages;
            var errorResponse = new OrchestratorResponse<InviteTeamMemberViewModel>
            {
                Data = model,
                FlashMessage = response.FlashMessage,
            };

            return View(errorResponse);
        }

        [HttpGet]
        [Route("invite/next")]
        public async Task<ActionResult> NextSteps(string hashedAccountId)
        {
            var userId = OwinWrapper.GetClaimValue(ControllerConstants.UserRefClaimKeyName);

            var userShownWizard = await _employerTeamOrchestrator.UserShownWizard(userId, hashedAccountId);

            var model = new OrchestratorResponse<InviteTeamMemberNextStepsViewModel>
            {
                FlashMessage = GetFlashMessageViewModelFromCookie(),
                Data = new InviteTeamMemberNextStepsViewModel
                {
                    UserShownWizard = userShownWizard
                }
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("invite/next")]
        public async Task<ActionResult> NextSteps(int? choice, string hashedAccountId)
        {
            var userId = OwinWrapper.GetClaimValue(ControllerConstants.UserRefClaimKeyName);

            var userShownWizard = await _employerTeamOrchestrator.UserShownWizard(userId, hashedAccountId);

            switch (choice ?? 0)
            {
                case 1: return RedirectToAction(ControllerConstants.InviteActionName);
                case 2: return RedirectToAction(ControllerConstants.ViewTeamActionName);
                case 3: return RedirectToAction(ControllerConstants.IndexActionName);
                default:
                    var model = new OrchestratorResponse<InviteTeamMemberNextStepsViewModel>
                    {
                        FlashMessage = GetFlashMessageViewModelFromCookie(),
                        Data = new InviteTeamMemberNextStepsViewModel
                        {
                            ErrorMessage = "You must select an option to continue.",
                            UserShownWizard = userShownWizard
                        }
                    };
                    return View(model); //No option entered
            }
        }

        [HttpGet]
        [Route("{invitationId}/cancel")]
        public async Task<ActionResult> Cancel(string email, string invitationId, string hashedAccountId)
        {
            var invitation = await _employerTeamOrchestrator.GetInvitation(invitationId);

            return View(invitation);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("{invitationId}/cancel")]
        public async Task<ActionResult> Cancel(string invitationId, string email, string hashedAccountId, int cancel)
        {
            if (cancel != 1)
                return RedirectToAction(ControllerConstants.ViewTeamViewName, new { HashedAccountId = hashedAccountId });

            var response = await _employerTeamOrchestrator.Cancel(email, hashedAccountId, OwinWrapper.GetClaimValue(ControllerConstants.UserRefClaimKeyName));

            return View(ControllerConstants.ViewTeamViewName, response);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("resend")]
        public async Task<ActionResult> Resend(string hashedAccountId, string email, string name)
        {
            var response = await _employerTeamOrchestrator.Resend(email, hashedAccountId, OwinWrapper.GetClaimValue(ControllerConstants.UserRefClaimKeyName), name);

            return View(ControllerConstants.ViewTeamViewName, response);
        }

        [HttpGet]
        [Route("{email}/remove/")]
        public async Task<ActionResult> Remove(string hashedAccountId, string email)
        {
            var response = await _employerTeamOrchestrator.Review(hashedAccountId, email);

            return View(response);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("{email}/remove")]
        public async Task<ActionResult> Remove(long userId, string hashedAccountId, string email, int remove)
        {
            Exception exception;
            HttpStatusCode httpStatusCode;

            try
            {
                if (remove != 1)
                    return RedirectToAction(ControllerConstants.ViewTeamViewName, new { HashedAccountId = hashedAccountId });

                var response = await _employerTeamOrchestrator.Remove(userId, hashedAccountId, OwinWrapper.GetClaimValue(ControllerConstants.UserRefClaimKeyName));

                return View(ControllerConstants.ViewTeamViewName, response);
            }
            catch (InvalidRequestException e)
            {
                httpStatusCode = HttpStatusCode.BadRequest;
                exception = e;
            }
            catch (UnauthorizedAccessException e)
            {
                httpStatusCode = HttpStatusCode.Unauthorized;
                exception = e;
            }

            var errorResponse = await _employerTeamOrchestrator.Review(hashedAccountId, email);
            errorResponse.Status = httpStatusCode;
            errorResponse.Exception = exception;

            return View(errorResponse);
        }

        [HttpGet]
        [Route("{email}/role/change")]
        public async Task<ActionResult> ChangeRole(string hashedAccountId, string email)
        {
            var teamMember = await _employerTeamOrchestrator.GetTeamMemberWhetherActiveOrNot(hashedAccountId, email, OwinWrapper.GetClaimValue(ControllerConstants.UserRefClaimKeyName));

            return View(teamMember);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("{email}/role/change")]
        public async Task<ActionResult> ChangeRole(string hashedAccountId, string email, Role role)
        {
            var response = await _employerTeamOrchestrator.ChangeRole(hashedAccountId, email, role, OwinWrapper.GetClaimValue(ControllerConstants.UserRefClaimKeyName));

            if (response.Status == HttpStatusCode.OK)
            {
                return View(ControllerConstants.ViewTeamViewName, response);
            }

            var teamMemberResponse = await _employerTeamOrchestrator.GetTeamMemberWhetherActiveOrNot(hashedAccountId, email, OwinWrapper.GetClaimValue(ControllerConstants.UserRefClaimKeyName));

            //We have to override flash message as the change role view has different model to view team view
            teamMemberResponse.FlashMessage = response.FlashMessage;
            teamMemberResponse.Exception = response.Exception;

            return View(teamMemberResponse);
        }

        [HttpGet]
        [Route("{email}/review/")]
        public async Task<ActionResult> Review(string hashedAccountId, string email)
        {
            var invitation = await _employerTeamOrchestrator.GetTeamMemberWhetherActiveOrNot(hashedAccountId, email, OwinWrapper.GetClaimValue(ControllerConstants.UserRefClaimKeyName));

            return View(invitation);
        }

        [HttpGet]
        [Route("hideWizard")]
        public async Task<ActionResult> HideWizard(string hashedAccountId)
        {
            var externalUserId = OwinWrapper.GetClaimValue(ControllerConstants.UserRefClaimKeyName);

            await _employerTeamOrchestrator.HideWizard(hashedAccountId, externalUserId);

            return RedirectToAction(ControllerConstants.IndexActionName);
        }

        [ChildActionOnly]
        public ActionResult Row1Panel1(NewHomepageViewModel model)
        {
            var viewModel = new PanelViewModel<NewHomepageViewModel> { ViewName = "CheckFunding", Data = model };
            if (model.Flags.AgreementsToSign)
            {
                viewModel.ViewName = "SignAgreement";
            }
            else if (model.EmulatedFundingViewModel != null)
            {
                viewModel.ViewName = "FundingComplete";
            }

            return PartialView(viewModel);
        }       

        [ChildActionOnly]
        public ActionResult Row1Panel2(NewHomepageViewModel model)
        {
            var viewModel = new PanelViewModel<NewHomepageViewModel> { ViewName = "ProviderPermissions", Data = model };
            if (model.Flags.AgreementsToSign)
            {
                viewModel.ViewName = "ProviderPermissionsDenied";
            }

            return PartialView(viewModel);
        }
        [ChildActionOnly]
        public ActionResult Row2Panel1(NewHomepageViewModel model)
        {
            return PartialView(new PanelViewModel<NewHomepageViewModel> { ViewName = "SavedProviders", Data = model });
        }
        [ChildActionOnly]
        public ActionResult Row2Panel2(NewHomepageViewModel model)
        {
            return PartialView(new PanelViewModel<NewHomepageViewModel> { ViewName = "CreateVacancy", Data = model });
        }

        [ChildActionOnly]
        public ActionResult SignAgreement(NewHomepageViewModel model)
        {
            return PartialView(model);
        }
        [ChildActionOnly]
        public ActionResult ProviderPermissions(NewHomepageViewModel model)
        {
            return PartialView(model);
        }
        [ChildActionOnly]
        public ActionResult ProviderPermissionsDenied(NewHomepageViewModel model)
        {
            return PartialView(model);
        }        
        [ChildActionOnly]
        public ActionResult SavedProviders(NewHomepageViewModel model)
        {
            return PartialView(model);
        }
        [ChildActionOnly]
        public ActionResult AccountSettings(NewHomepageViewModel model)
        {
            return PartialView(model);
        }
        [ChildActionOnly]
        public ActionResult CheckFunding(NewHomepageViewModel model)
        {
            return PartialView(model);
        }
        [ChildActionOnly]
        public ActionResult CreateVacancy(NewHomepageViewModel model)
        {
            return PartialView(model);
        }

        private async Task<OrchestratorResponse<AccountDashboardViewModel>> GetAccountInformation(string hashedAccountId)
        {
            var externalUserId = OwinWrapper.GetClaimValue(ControllerConstants.UserRefClaimKeyName);
            var response = await _employerTeamOrchestrator.GetAccount(hashedAccountId, externalUserId);
            var flashMessage = GetFlashMessageViewModelFromCookie();

            if (flashMessage != null)
            {
                response.FlashMessage = flashMessage;
                response.Data.EmployerAccountType = flashMessage.HiddenFlashMessageInformation;
            }

            return response;
        }

        public ActionResult ReturnFromEmulateFundingJourney(EmulatedFundingViewModel model)
        {
            _emulatedFundingViewModel = model;
            return RedirectToAction("Index", "EmployerTeam", new { model.HashedAccountId });
        }
        [ChildActionOnly]
        public ActionResult FundingComplete(AccountDashboardViewModel model)
        {
            return PartialView(model);
        }
    }
}