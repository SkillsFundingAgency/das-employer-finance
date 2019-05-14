﻿using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.Client.Interfaces;
using SFA.DAS.Commitments.Api.Types.Commitment;
using SFA.DAS.EAS.Portal.Application.Commands.Cohort;
using SFA.DAS.EAS.Portal.UnitTests.Builders;
using System.Threading.Tasks;
using FluentAssertions;
using System.Linq;
using System;
using System.Collections.Generic;
using SFA.DAS.EAS.Portal.Application.Services;
using System.Threading;
using SFA.DAS.EAS.Portal.Types;

namespace SFA.DAS.EAS.Portal.UnitTests.Application.Commands.Cohort
{
    [TestFixture]
    public class CohortApprovalRequestedCommandHandlerTests
    {
        public class TestContext
        {
            public CohortApprovalRequestedCommandHandler Sut { get; private set; }
            public Database.Models.Account TestAccount { get; private set; }
            public CommitmentView TestCommitment { get; private set; }
            public Mock<IAccountsService> MockAccountsService { get; private set; }
            public Mock<IProviderCommitmentsApi> MockProviderCommitmentsApi { get; private set; }

            public TestContext()
            {
                TestAccount = new AccountBuilder().WithOrganisation(new OrganisationBuilder());
                TestCommitment = new CommitmentViewBuilder();

                MockAccountsService = new Mock<IAccountsService>();

                MockAccountsService
                    .Setup(m => m.Get(It.IsAny<long>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(TestAccount);

                MockProviderCommitmentsApi = new Mock<IProviderCommitmentsApi>();

                MockProviderCommitmentsApi
                    .Setup(m => m.GetProviderCommitment(It.IsAny<long>(), It.IsAny<long>()))
                    .ReturnsAsync(TestCommitment);

                Sut = new CohortApprovalRequestedCommandHandler(MockAccountsService.Object, MockProviderCommitmentsApi.Object);
            }
        }

        public class Handle : CohortApprovalRequestedCommandHandlerTests
        {
            [Test]
            public async Task WhenCalled_ThenTheAccountServiceIsCalledToRetrieveTheAccount()
            {
                // arrange
                var testContext = new TestContext();
                CohortApprovalRequestedCommand command = new CohortApprovalRequestedCommandBuilder();

                // act
                await testContext.Sut.Handle(command);

                //assert
                testContext.MockAccountsService.Verify(m => m.Get(command.AccountId, It.IsAny<CancellationToken>()), Times.Once);
            }

            [Test]
            public async Task WhenCalled_ThenTheCommitmentsServiceIsCalledToRetrieveTheCommitmentForTheProvider()
            {
                // arrange
                var testContext = new TestContext();
                CohortApprovalRequestedCommand command = new CohortApprovalRequestedCommandBuilder();

                // act
                await testContext.Sut.Handle(command);

                //assert
                testContext.MockProviderCommitmentsApi.Verify(m => m.GetProviderCommitment(command.ProviderId, command.CommitmentId), Times.Once);
            }

            [Test]
            public async Task WhenCalled_ThenTheAccountServiceIsCalledToSaveTheAccount()
            {
                // arrange
                var testContext = new TestContext();
                CohortApprovalRequestedCommand command = new CohortApprovalRequestedCommandBuilder();

                // act
                await testContext.Sut.Handle(command);

                //assert
                testContext.MockAccountsService.Verify(m => m.Save(command.MessageId, testContext.TestAccount, It.IsAny<CancellationToken>()), Times.Once);
            }

            [Test]
            public async Task WhenCalledForANewCohort_ThenTheNewCohortIsSavedAgainstTheAccount()
            {
                // arrange
                var testContext = new TestContext();

                string cohortReference = Guid.NewGuid().ToString();
                testContext.TestCommitment.Reference = cohortReference;
                testContext.TestAccount.Organisations.First().Cohorts.Count.Should().Be(0);

                CohortApprovalRequestedCommand command = new CohortApprovalRequestedCommandBuilder();

                // act
                await testContext.Sut.Handle(command);

                //assert
                testContext.MockAccountsService.Verify(m =>
                m.Save(command.MessageId, It.Is<Database.Models.Account>(a =>
                a.Organisations.First().Cohorts.Count.Equals(1) &&
                a.Organisations.First().Cohorts.ToList().SingleOrDefault(c => c.Id.Equals(cohortReference)) != null), It.IsAny<CancellationToken>()),
                Times.Once);
            }

            [Test]
            public async Task WhenCalledForAnExistingCohort_ThenTheCohortCountIsNotChanged()
            {
                // arrange
                var testContext = new TestContext();
                string cohortReference = Guid.NewGuid().ToString();
                Types.Cohort cohort = new CohortBuilder().WithId(cohortReference);
                testContext.TestAccount.Organisations.First().Cohorts.Add(cohort);
                testContext.TestCommitment.Reference = cohortReference;
                testContext.TestAccount.Organisations.First().Cohorts.Count.Should().Be(1);

                CohortApprovalRequestedCommand command = new CohortApprovalRequestedCommandBuilder();

                // act
                await testContext.Sut.Handle(command);

                //assert
                testContext.MockAccountsService.Verify(m =>
                m.Save(command.MessageId, It.Is<Database.Models.Account>(a =>
                a.Organisations.First().Cohorts.Count.Equals(1)), It.IsAny<CancellationToken>()),
                Times.Once);
            }


            [Test]
            public async Task WhenANewCohortIsAdded_ThenApprentishipsInTheCohortAreStored()
            {
                // arrange
                var testContext = new TestContext();
                string cohortReference = Guid.NewGuid().ToString();
                long apprenticeshipId = 123;

                testContext.TestCommitment.Reference = cohortReference;
                testContext.TestCommitment.Apprenticeships = new List<Commitments.Api.Types.Apprenticeship.Apprenticeship>() { new Commitments.Api.Types.Apprenticeship.Apprenticeship { Id = apprenticeshipId } };

                CohortApprovalRequestedCommand command = new CohortApprovalRequestedCommandBuilder();

                // act
                await testContext.Sut.Handle(command);

                //assert
                testContext.MockAccountsService.Verify(m =>
                m.Save(command.MessageId, It.Is<Database.Models.Account>(a =>
                a.Organisations.First().Cohorts.Count.Equals(1) &&
                a.Organisations.First().Cohorts.First().Apprenticeships.Count.Equals(1) &&
                a.Organisations.First().Cohorts.First().Apprenticeships.First().Id.Equals(apprenticeshipId)), It.IsAny<CancellationToken>()),
                Times.Once);
            }

            [Test]
            public async Task WhenAnExistingCohortIsHandled_ThenAnyApprentishipsChangesAreAlsoStored()
            {
                // arrange
                var testContext = new TestContext();
                string cohortReference = Guid.NewGuid().ToString();
                Apprenticeship apprenticeship = new ApprenticeshipBuilder();
                Types.Cohort cohort = new CohortBuilder()
                    .WithId(cohortReference)
                    .WithApprenticeship(apprenticeship);
                testContext.TestAccount.Organisations.First().Cohorts.Add(cohort);
                testContext.TestAccount.Organisations.First().Cohorts.Count.Should().Be(1);

                testContext.TestCommitment.Reference = cohortReference;
                var testApprenticeship = new Commitments.Api.Types.Apprenticeship.Apprenticeship
                {
                    Id = apprenticeship.Id,
                    FirstName = Guid.NewGuid().ToString(),
                    LastName = Guid.NewGuid().ToString(),
                    Cost = 123.45M,
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddDays(10),
                    TrainingName = Guid.NewGuid().ToString()
                };
                testContext.TestCommitment.Apprenticeships.Add(testApprenticeship);

                testContext.TestAccount.Organisations.First().Cohorts.First().Apprenticeships.Count.Should().Be(1);

                CohortApprovalRequestedCommand command = new CohortApprovalRequestedCommandBuilder();

                // act
                await testContext.Sut.Handle(command);

                //assert
                testContext.MockAccountsService.Verify(m =>
                m.Save(command.MessageId, It.Is<Database.Models.Account>(a =>
                a.Organisations.First().Cohorts.First().Apprenticeships.Count.Equals(1) &&
                (a.Organisations.First().Cohorts.First().Apprenticeships.First().Id == testApprenticeship.Id) &&
                a.Organisations.First().Cohorts.First().Apprenticeships.First().FirstName.Equals(testApprenticeship.FirstName) &&
                a.Organisations.First().Cohorts.First().Apprenticeships.First().LastName.Equals(testApprenticeship.LastName) &&
                a.Organisations.First().Cohorts.First().Apprenticeships.First().CourseName.Equals(testApprenticeship.TrainingName) &&
                a.Organisations.First().Cohorts.First().Apprenticeships.First().StartDate.Equals(testApprenticeship.StartDate) &&
                a.Organisations.First().Cohorts.First().Apprenticeships.First().EndDate.Equals(testApprenticeship.EndDate) &&
                a.Organisations.First().Cohorts.First().Apprenticeships.First().ProposedCost.Equals(testApprenticeship.Cost)
                ), It.IsAny<CancellationToken>()),
                Times.Once);
            }
        }
    }
}
