﻿using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Models.Levy;
using SFA.DAS.EmployerFinance.Queries.GetLevyDeclaration;
using SFA.DAS.EmployerFinance.Validation;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerFinance.UnitTests.Queries.GetLevyDeclarationTests
{
    public class WhenIGetLevyDeclarations : QueryBaseTest<GetLevyDeclarationQueryHandler, GetLevyDeclarationRequest, GetLevyDeclarationResponse>
    {
        private Mock<IDasLevyRepository> _repository;
        private Mock<IEncodingService> _encodingService;
        public override GetLevyDeclarationRequest Query { get; set; }
        public override GetLevyDeclarationQueryHandler RequestHandler { get; set; }
        public override Mock<IValidator<GetLevyDeclarationRequest>> RequestValidator { get; set; }
        private const long ExpectedAccountId = 757361;
        private List<LevyDeclarationItem> _expectedLevyDeclarationViews;

        [SetUp]
        public void Arrange()
        {
            SetUp();

            Query = new GetLevyDeclarationRequest { HashedAccountId = "ABC123" };

            _encodingService = new Mock<IEncodingService>();
            _encodingService.Setup(x => x.Decode(Query.HashedAccountId,EncodingType.AccountId)).Returns(ExpectedAccountId);

            _repository = new Mock<IDasLevyRepository>();
            _expectedLevyDeclarationViews = LevyDeclarationViewsObjectMother.Create(ExpectedAccountId);
            _repository.Setup(x => x.GetAccountLevyDeclarations(It.IsAny<long>())).ReturnsAsync(_expectedLevyDeclarationViews);

            RequestHandler = new GetLevyDeclarationQueryHandler(_repository.Object, RequestValidator.Object, _encodingService.Object);
        }

        [Test]
        public override async Task ThenIfTheMessageIsValidTheRepositoryIsCalled()
        {
            //Act
            await RequestHandler.Handle(Query, CancellationToken.None);

            //Assert
            _repository.Verify(x => x.GetAccountLevyDeclarations(ExpectedAccountId), Times.Once);
        }

        [Test]
        public override async Task ThenIfTheMessageIsValidTheValueIsReturnedInTheResponse()
        {
            //Act
            var actual = await RequestHandler.Handle(Query, CancellationToken.None);

            //Assert
            actual.Should().NotBeNull();
            actual.Declarations.Should().BeEquivalentTo(_expectedLevyDeclarationViews);
        }
    }

    public static class LevyDeclarationViewsObjectMother
    {
        public static List<LevyDeclarationItem> Create(long accountId = 1234588, string empref = "123/abc123")
        {
            var item = new LevyDeclarationItem
            {
                Id = 95875,
                AccountId = accountId,
                LevyDueYtd = 1000,
                EmpRef = empref,
                EnglishFraction = 0.90m,
                PayrollMonth = 2,
                PayrollYear = "17-18",
                SubmissionDate = new DateTime(2016, 05, 14),
                SubmissionId = 1542,
                CreatedDate = DateTime.Now.AddDays(-1),
                DateCeased = null,
                EndOfYearAdjustment = false,
                EndOfYearAdjustmentAmount = 0,
                HmrcSubmissionId = 45,
                InactiveFrom = null,
                InactiveTo = null,
                LastSubmission = 1,
                LevyAllowanceForYear = 10000,
                TopUp = 100,
                TopUpPercentage = 0.1m,
                TotalAmount = 435,
                LevyDeclaredInMonth = 34857
            };

            return new List<LevyDeclarationItem> { item };
        }
    }
}
