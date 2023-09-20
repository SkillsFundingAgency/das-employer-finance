using Moq;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.EAS.Account.Api.Types;
using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Infrastructure.OuterApiRequests.Accounts;
using SFA.DAS.EmployerFinance.Infrastructure.OuterApiResponses.Accounts;
using SFA.DAS.EmployerFinance.Interfaces.OuterApi;
using SFA.DAS.EmployerFinance.Models.Account;
using SFA.DAS.EmployerFinance.Models.UserProfile;
using SFA.DAS.Encoding;
using SFA.DAS.Notifications.Api.Client;

namespace SFA.DAS.EmployerFinance.MessageHandlers.UnitTests.EventHandlers
{
    public class TransferConnectionRequestEventNotificationHandlerTestsFixtureBase
    {
        private readonly GetAccountTeamMembersWhichReceiveNotificationsResponse _getAccountTeamMembersWhichReceiveNotificationsResponse;
        
        public EmployerFinanceConfiguration Configuration { get; }
        public Mock<IEncodingService> EncodingService { get; }
        protected Mock<IOuterApiClient> OuterApiClient { get; }
        public Mock<INotificationsApi> NotificationsApiClient { get; }
        public Account SenderAccount { get; private set; }
        public Account ReceiverAccount { get; private set; }
        public User SenderAccountOwner1 { get; private set; }
        private User SenderAccountOwner2 { get; set; }
        public User ReceiverAccountOwner { get; private set; }

        public const string ReceiverHashedId = "ZZZZZSAAA";
        public const string SenderHashedId = "HHBBAGSTSTA";
        
        protected TransferConnectionRequestEventNotificationHandlerTestsFixtureBase()
        {
            Configuration = new EmployerFinanceConfiguration { EmployerFinanceBaseUrl = "https://www.example.test/" };
            OuterApiClient = new Mock<IOuterApiClient>();
            NotificationsApiClient = new Mock<INotificationsApi>();
            EncodingService = new Mock<IEncodingService>();

            _getAccountTeamMembersWhichReceiveNotificationsResponse = new GetAccountTeamMembersWhichReceiveNotificationsResponse();
            
            OuterApiClient
                .Setup(s => s.Get<GetAccountTeamMembersWhichReceiveNotificationsResponse>(
                    It.IsAny<GetAccountTeamMembersWhichReceiveNotificationsRequest>()))
                .ReturnsAsync(_getAccountTeamMembersWhichReceiveNotificationsResponse);

            EncodingService.Setup(s => s.Encode(It.Is<long>(x => x == ReceiverAccount.Id), EncodingType.AccountId)).Returns(ReceiverHashedId);
            EncodingService.Setup(s => s.Encode(It.Is<long>(x => x == SenderAccount.Id), EncodingType.AccountId)).Returns(SenderHashedId);
        }

        protected void AddSenderAccount()
        {
            SenderAccount = new Account
            {
                Id = 111111,
                Name = "SenderAccountName"
            };
        }

        protected void AddReceiverAccount()
        {
            ReceiverAccount = new Account
            {
                Id = 2222222,
                Name = "ReceiverAccountName",
                HashedId = "AHVVATS"
            };
        }

        protected void SetReceiverAccountOwner()
        {
            ReceiverAccountOwner = new User
            {
                UserRef = "123",
                FirstName = "Johnreceiver",
                Email = "JohnDoereceiver@zzzzzzzz.com"
            };

            AddTeamMember(ReceiverAccountOwner);
        }

        protected void SetSenderAccountOwner1()
        {
            SenderAccountOwner1 = new User
            {
                UserRef = "456",
                FirstName = "Johnsender",
                Email = "JohnDoesender@zzzzzzzz.com"
            };

            AddTeamMember(SenderAccountOwner1);
        }

        protected void SetSenderAccountOwner2()
        {
            SenderAccountOwner2 = new User
            {
                UserRef = "789",
                FirstName = "Johnsender2",
                Email = "JohnDoesender2@zzzzzzzz.com"
            };

            AddTeamMember(SenderAccountOwner2);
        }

        private void AddTeamMember(User user)
        {
            _getAccountTeamMembersWhichReceiveNotificationsResponse.Add(new TeamMember
            {
                UserRef = user.UserRef,
                FirstName = user.FirstName,
                Email = user.Email
            });
        }
    }
}