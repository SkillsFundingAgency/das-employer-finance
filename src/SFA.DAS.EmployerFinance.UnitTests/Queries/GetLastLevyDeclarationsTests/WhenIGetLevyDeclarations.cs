﻿using SFA.DAS.EmployerFinance.Models.Levy;
using SFA.DAS.EmployerFinance.Queries.GetLastLevyDeclaration;
using SFA.DAS.EmployerFinance.Validation;
using SFA.DAS.EmployerFinance.Data.Contracts;

namespace SFA.DAS.EmployerFinance.UnitTests.Queries.GetLastLevyDeclarationsTests
{
    public class WhenIGetLevyDeclarations : QueryBaseTest<GetLastLevyDeclarationQueryHandler, GetLastLevyDeclarationQuery, GetLastLevyDeclarationResponse>
    {
        private Mock<IDasLevyRepository> _dasLevyRepository;
        private const string ExpectedEmpref = "45TGB";

        public override GetLastLevyDeclarationQuery Query { get; set; }
        public override GetLastLevyDeclarationQueryHandler RequestHandler { get; set; }
        public override Mock<IValidator<GetLastLevyDeclarationQuery>> RequestValidator { get; set; }

        [SetUp]
        public void Arrange()
        {
            SetUp();
            
            _dasLevyRepository = new Mock<IDasLevyRepository>();

            Query = new GetLastLevyDeclarationQuery { EmpRef = ExpectedEmpref };

            RequestHandler = new GetLastLevyDeclarationQueryHandler(RequestValidator.Object, _dasLevyRepository.Object);
        }
        
        [Test]
        public override async Task ThenIfTheMessageIsValidTheRepositoryIsCalled()
        {
            //Act
            await RequestHandler.Handle(Query, CancellationToken.None);

            //Assert
            _dasLevyRepository.Verify(x => x.GetLastSubmissionForScheme(ExpectedEmpref));
        }

        [Test]
        public override async Task ThenIfTheMessageIsValidTheValueIsReturnedInTheResponse()
        {
            //Arrange
            var expectedDate = new DateTime(2016, 01, 29);
            _dasLevyRepository.Setup(x => x.GetLastSubmissionForScheme(ExpectedEmpref)).ReturnsAsync(new DasDeclaration { SubmissionDate = expectedDate });

            //Act
            var actual = await RequestHandler.Handle(Query,CancellationToken.None);

            //Assert
            actual.Transaction.Should().NotBeNull();
            actual.Transaction.SubmissionDate.Should().Be(expectedDate);
        }
        
    }
}
