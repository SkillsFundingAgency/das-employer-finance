﻿using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.Client.Interfaces;
using SFA.DAS.EAS.Application.Queries.GetTransferConnectionInvitations;
using SFA.DAS.EAS.Domain.Configuration;
using SFA.DAS.EAS.Domain.Interfaces;
using SFA.DAS.EAS.Domain.Models.Account;
using SFA.DAS.EAS.Infrastructure.DependencyResolution;
using SFA.DAS.EAS.TestCommon.DependencyResolution;
using SFA.DAS.EAS.Web.Authentication;
using SFA.DAS.EAS.Web.Controllers;
using SFA.DAS.EAS.Web.ViewModels.Transfers;
using SFA.DAS.Events.Api.Client;
using SFA.DAS.Messaging.Interfaces;
using StructureMap;
using System;
using System.Web.Mvc;
using TechTalk.SpecFlow;

namespace SFA.DAS.EAS.Transactions.AcceptanceTests.Steps.TransferDashboardSteps
{
    [Binding]
    public class TransferDashboardSteps : TechTalk.SpecFlow.Steps
    {

        private static IContainer _container;
        private static Mock<IMessagePublisher> _messagePublisher;
        private static Mock<IAuthenticationService> _owinWrapper;
        private string _hashedAccountId;
        private static Mock<ICookieStorageService<EmployerAccountData>> _cookieService;
        private static Mock<IEventsApi> _eventsApi;
        private static Mock<IEmployerCommitmentApi> _commitmentsApi;
        private static LevyDeclarationProviderConfiguration _levyDeclarationProviderConfiguration;


        [BeforeFeature]
        public static void Arrange()
        {
            _messagePublisher = new Mock<IMessagePublisher>();
            _owinWrapper = new Mock<IAuthenticationService>();
            _cookieService = new Mock<ICookieStorageService<EmployerAccountData>>();
            _eventsApi = new Mock<IEventsApi>();
            _commitmentsApi = new Mock<IEmployerCommitmentApi>();

            _levyDeclarationProviderConfiguration =
                ConfigurationHelper.GetConfiguration<LevyDeclarationProviderConfiguration>(IoC.LevyAggregationProviderName);

            _container = IoC.CreateContainer(
                _messagePublisher,
                _owinWrapper,
                _cookieService,
                _eventsApi,
                _commitmentsApi,
                _levyDeclarationProviderConfiguration);
        }

        [AfterFeature]
        public static void TearDown()
        {
            _container.Dispose();
        }

        [When(@"The transfer allowance ratio is (.*) percent")]
        public void WhenTheTransferAllowanceRatioIsPercent(int percentage)
        {
            _levyDeclarationProviderConfiguration.TransferAllowancePercentage = percentage / 100f;
        }


        [Then(@"the transfer allowance should be (.*) on the transfer dashboard screen")]
        public void ThenTheTransferAllowanceShouldBeOnTheTransferDashboardScreen(decimal expectedTransferBalance)
        {
            var hashedAccountId = ScenarioContext.Current["HashedAccountId"].ToString();
            var accountId = (long)ScenarioContext.Current["AccountId"];
            var externalUserId = Guid.Parse(ScenarioContext.Current["AccountOwnerUserId"].ToString());

            var controller = _container.GetInstance<TransfersController>();
            var view = controller.Index(new GetTransferConnectionInvitationsQuery
            {
                AccountHashedId = hashedAccountId,
                AccountId = accountId,
                UserExternalId = externalUserId
            }).Result as ViewResult;

            var viewModel = view?.Model as TransferConnectionInvitationsViewModel;

            Assert.IsNotNull(viewModel);

            Assert.AreEqual(expectedTransferBalance.ToString("C0"), viewModel.TransferAllowance.ToString("C0"));
        }
    }
}

