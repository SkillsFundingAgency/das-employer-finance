﻿using System.Configuration;
using System.Web.Mvc;
using SFA.DAS.Authorization.Mvc.Attributes;
using SFA.DAS.EAS.Domain.Configuration;
using SFA.DAS.EAS.Web.Attributes;
using SFA.DAS.EAS.Web.Helpers;
using SFA.DAS.EAS.Web.ViewModels;

namespace SFA.DAS.EAS.Web.Controllers
{
    [DasAuthorize]
    [RoutePrefix("accounts/{hashedaccountId}/apprentices")]
    public class EmployerCommitmentsController : Controller
    {
        private readonly EmployerApprenticeshipsServiceConfiguration _easConfig;

        public EmployerCommitmentsController(EmployerApprenticeshipsServiceConfiguration easConfig) : base()
        {
            _easConfig = easConfig;
        }

        [HttpGet]
        [Route("home", Name = "CommitmentsHome")]
        [Route("cohorts/review")]
        [Route("cohorts/provider")]
        [Route("Inform")]
        [Route("confirmProvider/create")]
        [Deprecated]
        public ActionResult Index(string hashedAccountId)
        {
            return RedirectPermanentCommitmentsUrl();
        }

        [HttpGet]
        [OutputCache(CacheProfile = "NoCache")]
        [Route("cohorts")]
        [Route("cohorts/draft")]
        [Deprecated]
        public ActionResult YourCohorts(string hashedAccountId)
        {
            return RedirectPermanentCommitmentsUrl();
        }

        [HttpGet]
        [Route("legalEntity/create")]
        [Deprecated]
        public ActionResult SelectLegalEntity(string hashedAccountId, string cohortRef = "")
        {
            return RedirectPermanentCommitmentsUrl();
        }

        [HttpGet]
        [Route("provider/create")]
        [Deprecated]
        public ActionResult SearchProvider(string hashedAccountId, string legalEntityCode, string cohortRef)
        {
            return RedirectPermanentCommitmentsUrl();
        }

        [HttpGet]
        [Route("choosePath/create")]
        [Deprecated]
        public ActionResult ChoosePath(string hashedAccountId, string legalEntityCode, string providerId, string cohortRef)
        {
            return RedirectPermanentCommitmentsUrl();
        }

        [HttpGet]
        [OutputCache(CacheProfile = "NoCache")]
        [Route("{hashedCommitmentId}/details")]
        [Route("{hashedCommitmentId}/details/delete")]
        [Route("{hashedCommitmentId}/apprenticeships/create")]
        [Route("{hashedCommitmentId}/finished")]
        [Route("{hashedCommitmentId}/CohortApproved")]
        [Deprecated]
        public ActionResult Details(string hashedAccountId, string hashedCommitmentId)
        {
            return RedirectPermanentCommitmentsUrl();
        }

        [HttpGet]
        [Route("{legalEntityCode}/AgreementNotSigned")]
        [Deprecated]
        public ActionResult AgreementNotSigned(LegalEntitySignedAgreementViewModel viewModel)
        {
            return RedirectPermanentCommitmentsUrl();
        }

        [HttpGet]
        [OutputCache(CacheProfile = "NoCache")]
        [Route("{hashedCommitmentId}/apprenticeships/{hashedApprenticeshipId}/edit")]
        [Route("{hashedCommitmentId}/apprenticeships/{hashedApprenticeshipId}/view")]
        [Route("{hashedCommitmentId}/Apprenticeships/{hashedApprenticeshipId}/Delete")]
        [Deprecated]
        public ActionResult EditApprenticeship(string hashedAccountId, string hashedCommitmentId, string hashedApprenticeshipId)
        {
            return RedirectPermanentCommitmentsUrl();
        }

        [HttpGet]
        [OutputCache(CacheProfile = "NoCache")]
        [Route("{hashedCommitmentId}/submit")]
        [Deprecated]
        public ActionResult SubmitExistingCommitment(string hashedAccountId, string hashedCommitmentId)
        {
            return RedirectPermanentCommitmentsUrl();
        }

        [HttpGet]
        [OutputCache(CacheProfile = "NoCache")]
        [Route("Submit")]
        [Deprecated]
        public ActionResult SubmitNewCommitment(string hashedAccountId, string legalEntityCode, string legalEntityName, string legalEntityAddress, short legalEntitySource, string providerId, string providerName, string cohortRef)
        {
            return RedirectPermanentCommitmentsUrl();
        }

        [HttpGet]
        [Route("{hashedCommitmentId}/NewCohortAcknowledgement")]
        [Deprecated]
        public ActionResult AcknowledgementNew(string hashedAccountId, string hashedCommitmentId)
        {
            return RedirectPermanentCommitmentsUrl();
        }

        [HttpGet]
        [Route("{hashedCommitmentId}/Acknowledgement")]
        [Deprecated]
        public ActionResult AcknowledgementExisting(string hashedAccountId, string hashedCommitmentId)
        {
            return RedirectPermanentCommitmentsUrl();
        }

        private RedirectResult RedirectPermanentCommitmentsUrl()
        {
            var baseUrl = _easConfig.EmployerCommitmentsBaseUrl.EndsWith("/")
                ? _easConfig.EmployerCommitmentsBaseUrl
                : _easConfig.EmployerCommitmentsBaseUrl + "/";

            var path = Request.Url.AbsolutePath;

            return RedirectPermanent($"{baseUrl}{path}");
        }
    }
}