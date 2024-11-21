using AutoMapper;
using SFA.DAS.EmployerFinance.Models.Payments;
using SFA.DAS.Provider.Events.Api.Types;
using StructureMap.TypeRules;
using Payment = SFA.DAS.Provider.Events.Api.Types.Payment;

namespace SFA.DAS.EmployerFinance.UnitTests.Mappings
{
    class WhenIMapAPaymentToPaymentDetails
    {
        private IMapper _mapper;

        [SetUp]
        public void Arrange()
        {
            var profiles = AppDomain.CurrentDomain
                .GetAssemblies()
                .Where(a => a.FullName.StartsWith("SFA.DAS.EmployerFinance"))
                .SelectMany(a => a.GetTypes())
                .Where(t => typeof(Profile).IsAssignableFrom(t) && t.IsConcrete() && t.HasConstructors())
                .Select(t => (Profile)Activator.CreateInstance(t))
                .ToList();

            var config = new MapperConfiguration(c =>
            {
                profiles.ForEach(c.AddProfile);
            });

            _mapper = config.CreateMapper();
        }

        [Test]
        public void ThenAllDataShouldBeMappedCorrectly()
        {
            //Arrange
            var payment = new Payment
            {
                Id = Guid.NewGuid().ToString(),
                Ukprn = 321231,
                Amount = 120.67m,
                ApprenticeshipId = 123,
                DeliveryPeriod = new CalendarPeriod { Month = 4, Year = 1956 },
                CollectionPeriod = new NamedCalendarPeriod { Id = "564", Month = 6, Year = 2018 },
                TransactionType = TransactionType.Learning,
                ApprenticeshipVersion = "1.1",
                EmployerAccountVersion = "1.2",
                ContractType = ContractType.ContractWithEmployer,
                EmployerAccountId = "7897",
                FrameworkCode = 12,
                EvidenceSubmittedOn = DateTime.Now,
                FundingSource = FundingSource.CoInvestedSfa,
                PathwayCode = 4,
                ProgrammeType = 3,
                StandardCode = 78,
                Uln = 5555
            };

            //Act
            var result = _mapper.Map<PaymentDetails>(payment);

            //Assert
            result.Id.Should().Be(Guid.Parse(payment.Id));
            payment.Ukprn.Should().Be(payment.Ukprn);
            payment.Amount.Should().Be(payment.Amount);
            payment.ApprenticeshipId.Should().Be(payment.ApprenticeshipId);
            result.DeliveryPeriodMonth.Should().Be(payment.DeliveryPeriod.Month);
            result.DeliveryPeriodYear.Should().Be(payment.DeliveryPeriod.Year);
            result.CollectionPeriodId.Should().Be(payment.CollectionPeriod.Id);
            result.CollectionPeriodMonth.Should().Be(payment.CollectionPeriod.Month);
            result.CollectionPeriodYear.Should().Be(payment.CollectionPeriod.Year);
            result.TransactionType.Should().Be(payment.TransactionType);
            result.ApprenticeshipVersion.Should().Be(payment.ApprenticeshipVersion);
            result.EmployerAccountVersion.Should().Be(payment.EmployerAccountVersion);
            result.EmployerAccountId.ToString().Should().Be(payment.EmployerAccountId);
            result.FrameworkCode.Should().Be(payment.FrameworkCode);
            result.EvidenceSubmittedOn.Should().Be(payment.EvidenceSubmittedOn);
            result.FundingSource.Should().Be(payment.FundingSource);
            result.PathwayCode.Should().Be(payment.PathwayCode);
            result.ProgrammeType.Should().Be(payment.ProgrammeType);
            result.StandardCode.Should().Be(payment.StandardCode);
            result.Uln.Should().Be(payment.Uln);
        }
    }
}
