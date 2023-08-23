using Moq;
using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Infrastructure.OuterApiRequests.Accounts;
using SFA.DAS.EmployerFinance.Infrastructure.OuterApiResponses.Accounts;
using SFA.DAS.EmployerFinance.Interfaces.OuterApi;
using SFA.DAS.EmployerFinance.Models.Account;
using SFA.DAS.EmployerFinance.Models.UserProfile;
using SFA.DAS.Notifications.Api.Client;

namespace SFA.DAS.EmployerFinance.MessageHandlers.UnitTests.EventHandlers
{
    public class TransferConnectionRequestEventNotificationHandlerTestsFixtureBase
    {
        public EmployerFinanceConfiguration Configuration { get; }
        protected Mock<IOuterApiClient> OuterApiClient { get; }
        public Mock<INotificationsApi> NotificationsApiClient { get; }

        private GetAccountTeamMembersWhichReceiveNotificationsResponse GetAccountTeamMembersWhichReceiveNotificationsResponse { get; }

        public Account SenderAccount { get; set; }
        public Account ReceiverAccount { get; set; }
        public User SenderAccountOwner1 { get; set; }
        private User SenderAccountOwner2 { get; set; }
        public User ReceiverAccountOwner { get; set; }

        protected TransferConnectionRequestEventNotificationHandlerTestsFixtureBase()
        {
            Configuration = new EmployerFinanceConfiguration{ EmployerFinanceBaseUrl = "https://www.example.test/"};
            OuterApiClient = new Mock<IOuterApiClient>();
            NotificationsApiClient = new Mock<INotificationsApi>();

            GetAccountTeamMembersWhichReceiveNotificationsResponse = new GetAccountTeamMembersWhichReceiveNotificationsResponse();

            OuterApiClient
                .Setup(s => s.Get<GetAccountTeamMembersWhichReceiveNotificationsResponse>(It.IsAny<GetAccountTeamMembersWhichReceiveNotificationsRequest>()))
                .ReturnsAsync(GetAccountTeamMembersWhichReceiveNotificationsResponse);
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
                HashedId = "ZYXA222"
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
            GetAccountTeamMembersWhichReceiveNotificationsResponse.Add(new TeamMember
            {
                UserRef = user.UserRef,
                FirstName = user.FirstName,
                Email = user.Email
            });
        }
    }
}