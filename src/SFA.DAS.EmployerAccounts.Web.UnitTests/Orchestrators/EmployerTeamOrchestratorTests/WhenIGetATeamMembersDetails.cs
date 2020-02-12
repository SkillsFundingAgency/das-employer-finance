﻿using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.Authorization.Services;
using SFA.DAS.CommitmentsV2.Api.Client;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.EmployerAccounts.Interfaces;
using SFA.DAS.EmployerAccounts.Models;
using SFA.DAS.EmployerAccounts.Models.AccountTeam;
using SFA.DAS.EmployerAccounts.Queries.GetMember;
using SFA.DAS.EmployerAccounts.Queries.GetUserAccountRole;
using SFA.DAS.EmployerAccounts.Web.Orchestrators;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerAccounts.Web.UnitTests.Orchestrators.EmployerTeamOrchestratorTests
{
    class WhenIGetATeamMembersDetails
    {
        private const string ExternalUserId = "123ABC";
        private const string TeamMemberEmail = "test@test.com";
        private const string HashedAccountId = "ABC123";

        private Mock<IMediator> _mediator;
        private Mock<IAccountApiClient> _accountApiClient;
        private Mock<ICommitmentsApiClient> _commitmentsApiClient;
        private Mock<IEncodingService> _encodingService;
        private Mock<IMapper> _mapper;
        private EmployerTeamOrchestrator _orchestrator;
        private GetMemberResponse _teamMemberResponse;

        [SetUp]
        public void Arrange()
        {
            _teamMemberResponse = new GetMemberResponse
            {
                TeamMember = new TeamMember
                {
                    AccountId = 1,
                    Email = TeamMemberEmail,
                    Role = Role.Owner
                }
            };

            _mediator = new Mock<IMediator>();
            _accountApiClient = new Mock<IAccountApiClient>();
            _commitmentsApiClient = new Mock<ICommitmentsApiClient>();
            _encodingService = new Mock<IEncodingService>();
            _mapper = new Mock<IMapper>();

            _orchestrator = new EmployerTeamOrchestrator(_mediator.Object, Mock.Of<ICurrentDateTime>(), _accountApiClient.Object, _commitmentsApiClient.Object, _encodingService.Object, _mapper.Object, Mock.Of<IAuthorizationService>());

            _mediator.Setup(x => x.SendAsync(It.IsAny<GetMemberRequest>()))
                .ReturnsAsync(_teamMemberResponse);
        }

        [TestCase(Role.Owner, HttpStatusCode.OK)]
        [TestCase(Role.Transactor, HttpStatusCode.Unauthorized)]
        [TestCase(Role.Viewer, HttpStatusCode.Unauthorized)]
        public async Task ThenOnlyOwnersShouldBeAbleToGetATeamMembersDetails(Role userRole, HttpStatusCode status)
        {
            //Arrange
            _mediator.Setup(x => x.SendAsync(It.IsAny<GetUserAccountRoleQuery>()))
                .ReturnsAsync(new GetUserAccountRoleResponse {UserRole = userRole});

            //Act
            var result = await _orchestrator.GetActiveTeamMember(HashedAccountId, TeamMemberEmail, ExternalUserId);

            //Assert
            _mediator.Verify(x => x.SendAsync(It.Is<GetUserAccountRoleQuery>(q => 
                        q.HashedAccountId.Equals(HashedAccountId) && 
                        q.ExternalUserId.Equals(ExternalUserId))), Times.Once);

            Assert.AreEqual(status, result.Status);
        }

        [Test]
        public async Task ThenAnOwnerShouldBeAbleToSeeTeamMemberDetails()
        {
            //Arrange
            _mediator.Setup(x => x.SendAsync(It.IsAny<GetUserAccountRoleQuery>()))
                .ReturnsAsync(new GetUserAccountRoleResponse { UserRole = Role.Owner });

            //Act
            var result = await _orchestrator.GetActiveTeamMember(HashedAccountId, TeamMemberEmail, ExternalUserId);

            //Assert
            _mediator.Verify(x => x.SendAsync(It.Is<GetMemberRequest>(r => 
                        r.HashedAccountId.Equals(HashedAccountId) &&
                        r.Email.Equals(TeamMemberEmail))), Times.Once);

            Assert.AreEqual(_teamMemberResponse.TeamMember, result.Data);
        }

        [TestCase(Role.Transactor)]
        [TestCase(Role.Viewer)]
        public async Task ThenUsersWhoAreNotOwnersShouldNotGetTeamMemberDetails(Role userRole)
        {
            //Arrange
            _mediator.Setup(x => x.SendAsync(It.IsAny<GetUserAccountRoleQuery>()))
                .ReturnsAsync(new GetUserAccountRoleResponse { UserRole = userRole });

            //Act
            await _orchestrator.GetActiveTeamMember(HashedAccountId, TeamMemberEmail, ExternalUserId);

            //Assert
            _mediator.Verify(x => x.SendAsync(It.IsAny<GetMemberRequest>()), Times.Never);
        }

        [Test]
        public async Task ThenItShouldReturnANotFoundIfNoTeamMembersAreFound()
        {
            //Arrange
            _mediator.Setup(x => x.SendAsync(It.IsAny<GetUserAccountRoleQuery>()))
                .ReturnsAsync(new GetUserAccountRoleResponse { UserRole = Role.Owner });
            _mediator.Setup(x => x.SendAsync(It.IsAny<GetMemberRequest>()))
                .ReturnsAsync(new GetMemberResponse
                {
                    TeamMember = new TeamMember()
                });

            //Act
            var result = await _orchestrator.GetActiveTeamMember(null, null, null);

            //Asset
            Assert.AreEqual(HttpStatusCode.NotFound,result.Status);
        }
    }
}
