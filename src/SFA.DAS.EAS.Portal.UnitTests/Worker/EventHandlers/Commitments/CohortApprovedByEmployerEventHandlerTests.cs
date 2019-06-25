﻿using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.Types.Commitment;
using SFA.DAS.EAS.Portal.Client.Database.Models;
using SFA.DAS.EAS.Portal.Worker.EventHandlers.Commitments;
using SFA.DAS.Testing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Messages.Events;

namespace SFA.DAS.EAS.Portal.UnitTests.Worker.EventHandlers.Commitments
{
    [TestFixture]
    [Parallelizable]
    public class CohortApprovedByEmployerEventHandlerTests : FluentTest<CohortApprovedByEmployerEventHandlerTestsFixture>
    {
        [Test]
        public Task Handle_WhenHandlingCohortApprovalRequestedByProvider_ThenShouldInitialiseMessageContext()
        {
            return TestAsync(f => f.Handle(), f => f.VerifyMessageContextIsInitialised());
        }

        [Test]
        public Task Handle_WhenAccountDoesContainOrganisationAndCohort_ThenAccountDocumentIsSavedWithUpdatedCohort()
        {
            return TestAsync(f => f.ArrangeAccountDocumentContainsCohort(), f => f.Handle(),
                f => f.VerifyAccountDocumentSavedWithCohortApproved());
        }
    }

    public class CohortApprovedByEmployerEventHandlerTestsFixture : EventHandlerTestsFixture<
        CohortApprovedByEmployer, CohortApprovedByEmployerEventHandler>
    {
        public CommitmentView Commitment { get; set; }
        public CommitmentView ExpectedCommitment { get; set; }
        public const long AccountLegalEntityId = 456L;

        public CohortApprovedByEmployerEventHandlerTestsFixture() : base(false)
        {
            Handler = new CohortApprovedByEmployerEventHandler(
                AccountDocumentService.Object,
                MessageContextInitialisation.Object,
                Logger.Object);
        }

        public CohortApprovedByEmployerEventHandlerTestsFixture ArrangeAccountDocumentContainsCohort()
        {
            var organisation = SetUpAccountDocumentWithOrganisation(Message.AccountId, AccountLegalEntityId);

            organisation.Cohorts.RandomElement().Id = Message.CommitmentId.ToString();
            
            AccountDocumentService.Setup(s => s.Get(Message.AccountId, It.IsAny<CancellationToken>())).ReturnsAsync(AccountDocument);
            
            return this;
        }

        public override Task Handle()
        {
            ExpectedCommitment = Commitment.Clone();

            return base.Handle();
        }
        
        public CohortApprovedByEmployerEventHandlerTestsFixture VerifyAccountDocumentSavedWithCohortApproved()
        {
            AccountDocumentService.Verify(
                s => s.Save(It.Is<AccountDocument>(d => CohortIsSetToApproved(d)),It.IsAny<CancellationToken>()), Times.Once);
            
            return this;
        }
        
        private bool CohortIsSetToApproved(AccountDocument document)
        {
            return document.Account.Organisations.SelectMany(org => org.Cohorts)
                .Single(co => co.Id == Message.CommitmentId.ToString()).IsApproved;
        }
    }
}