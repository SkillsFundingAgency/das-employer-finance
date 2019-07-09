using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.EAS.Portal.Client;
using SFA.DAS.EAS.Portal.Client.Application.Queries;
using SFA.DAS.EAS.Portal.Client.Services.DasRecruit;
using SFA.DAS.EAS.Portal.Client.Types;
using SFA.DAS.Testing;
using StructureMap;

namespace SFA.DAS.EAS.Portal.UnitTests.Client
{
    [TestFixture, Parallelizable, Ignore("WIP")]
    public class PortalClientTests : FluentTest<PortalClientTestsFixture>
    {
        //todo: account related tests, ie. account returned, account not returned
        
        [Test]
        public Task GetAccount_WhenHasNoPayeScheme_ThenVacancyCardinalityIsNotSet()
        {
            return TestAsync(f => f.ArrangeHasNoPayeScheme(), f => f.GetAccount(),
                (f, r) => f.AssertVacancyCardinalityIsNotSet(r));
        }

        [Test]
        public Task GetAccount_WhenHasNoPayeScheme_ThenSingleVacancyIsNotSet()
        {
            return TestAsync(f => f.GetAccount(), (f, r) => f.AssertSingleVacancyIsNotSet(r));
        }

        [Test]
        public Task GetAccount_WhenHasPayeSchemeAndQueryFails_ThenVacancyCardinalityIsNotSet()
        {
            return TestAsync(f => f.GetAccount(), (f, r) => f.AssertVacancyCardinalityIsNotSet(r));
        }

        [Test]
        public Task GetAccount_WhenHasPayeSchemeAndQueryFails_ThenSingleVacancyIsNotSet()
        {
            return TestAsync(f => f.GetAccount(), (f, r) => f.AssertSingleVacancyIsNotSet(r));
        }

        [Test]
        public Task GetAccount_WhenHasPayeSchemeAndQuerySucceedsAndReturnsNoVacancies_ThenVacancyCardinalityIsSetToNone()
        {
            return TestAsync(f => f.GetAccount(), (f, r) => f.AssertVacancyCardinalityIsSet(Cardinality.None));
        }

        [Test]
        public Task GetAccount_WhenHasPayeSchemeAndQuerySucceedsAndReturnsNoVacancies_ThenSingleVacancyIsNotSet()
        {
            return TestAsync(f => f.GetAccount(), (f, r) => f.AssertSingleVacancyIsNotSet(r));
        }
        
        [Test]
        public Task GetAccount_WhenHasPayeSchemeAndQuerySucceedsAndReturnsOneVacancies_ThenVacancyCardinalityIsSetToOne()
        {
            return TestAsync(f => f.GetAccount(),  (f, r) => f.AssertVacancyCardinalityIsSet(Cardinality.One));
        }

        [Test]
        public Task GetAccount_WhenHasPayeSchemeAndQuerySucceedsAndReturnsOneVacancies_ThenSingleVacancyIsSetCorrectly()
        {
            return TestAsync(f => f.GetAccount(), (f, r) => f.AssertSingleVacancyIsSetCorrectly(r));
        }

        [Test]
        public Task GetAccount_WhenHasPayeSchemeAndQuerySucceedsAndReturnsTwoVacancies_ThenVacancyCardinalityIsSetToMany()
        {
            return TestAsync(f => f.GetAccount(),  (f, r) => f.AssertVacancyCardinalityIsSet(Cardinality.Many));
        }

        [Test]
        public Task GetAccount_WhenHasPayeSchemeAndQuerySucceedsAndReturnsTwoVacancies_ThenSingleVacancyIsNotSet()
        {
            return TestAsync(f => f.GetAccount(), (f, r) => f.AssertSingleVacancyIsNotSet(r));
        }
    }

    public class PortalClientTestsFixture
    {
        PortalClient PortalClient { get; set; }
        Mock<IContainer> MockContainer { get; set; } = new Mock<IContainer>();
        Mock<IGetAccountQuery> MockGetAccountQuery { get; set; } = new Mock<IGetAccountQuery>();
        Mock<IDasRecruitService> MockDasRecruitService { get; set; } = new Mock<IDasRecruitService>();
        bool HasPayeScheme { get; set; } = true;
        Account Account { get; set; }
        IEnumerable<Vacancy> Vacancies { get; set; }
        const long AccountId = 999L;
        
        public PortalClientTestsFixture()
        {
            MockGetAccountQuery.Setup(q => q.Get(AccountId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Account);
            MockDasRecruitService.Setup(s => s.GetVacancies(AccountId, 2, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Vacancies);
            
            MockContainer.Setup(c => c.GetInstance<IGetAccountQuery>())
                .Returns(MockGetAccountQuery.Object);
            MockContainer.Setup(c => c.GetInstance<IDasRecruitService>())
                .Returns(MockDasRecruitService.Object);
            
            PortalClient = new PortalClient(MockContainer.Object);
        }

        public PortalClientTestsFixture ArrangeHasNoPayeScheme()
        {
            HasPayeScheme = false;
            
            return this;
        }
        
        public PortalClientTestsFixture ArrangeQueryFails()
        {
            return this;
        }

        public PortalClientTestsFixture ArrangeQuerySucceedsAndReturnsNoVacancies()
        {
            return this;
        }
        
        public PortalClientTestsFixture ArrangeQuerySucceedsAndReturnsOneVacancy()
        {
            return this;
        }

        public PortalClientTestsFixture ArrangeQuerySucceedsAndReturnsTwoVacancies()
        {
            return this;
        }

        public async Task<Account> GetAccount()
        {
            return await PortalClient.GetAccount(AccountId, HasPayeScheme);
        }

        public void AssertVacancyCardinalityIsNotSet(Account account)
        {
        }

        public void AssertVacancyCardinalityIsSet(Cardinality expectedCardinality)
        {
        }
        
        public void AssertSingleVacancyIsNotSet(Account account)
        {
        }

        public void AssertSingleVacancyIsSetCorrectly(Account account)
        {
        }
    }
}