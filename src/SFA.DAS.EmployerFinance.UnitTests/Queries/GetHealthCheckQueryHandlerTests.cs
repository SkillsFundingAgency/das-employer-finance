﻿using AutoMapper;
using SFA.DAS.EmployerFinance.Data;
using SFA.DAS.EmployerFinance.Dtos;
using SFA.DAS.EmployerFinance.Mappings;
using SFA.DAS.EmployerFinance.Models;
using SFA.DAS.EmployerFinance.Queries.GetHealthCheck;
using SFA.DAS.EmployerFinance.TestCommon.DatabaseMock;
using SFA.DAS.EmployerFinance.UnitTests.Builders;
using SFA.DAS.Testing;

namespace SFA.DAS.EmployerFinance.UnitTests.Queries
{

    [TestFixture]
    public class GetHealthCheckQueryHandlerTests : FluentTest<GetHealthCheckQueryHandlerTestsFixture>
    {
        [Test]
        public Task Handle_WhenHandlingAGetHealthCheckQuery_ThenShouldReturnAGetHealthCheckQueryResponse()
        {
            return TestAsync(f => f.Handle(), (f, r) =>
            {
                r.Should().NotBeNull();
                r.HealthCheck.Should().NotBeNull().And.Match<HealthCheckDto>(d => d.Id == f.HealthChecks[1].Id);
            });
        }
    }

    public class GetHealthCheckQueryHandlerTestsFixture
    {
        public GetHealthCheckQuery GetHealthCheckQuery { get; set; }
        public IRequestHandler<GetHealthCheckQuery, GetHealthCheckQueryResponse> Handler { get; set; }
        public Mock<EmployerFinanceDbContext> Db { get; set; }
        public IConfigurationProvider ConfigurationProvider { get; set; }
        public List<HealthCheck> HealthChecks { get; set; }

        public GetHealthCheckQueryHandlerTestsFixture()
        {
            GetHealthCheckQuery = new GetHealthCheckQuery();
            Db = new Mock<EmployerFinanceDbContext>();
            ConfigurationProvider = new MapperConfiguration(c => c.AddProfile<HealthCheckMappings>());

            HealthChecks = new List<HealthCheck>
            {
                new HealthCheckBuilder().WithId(1).Build(),
                new HealthCheckBuilder().WithId(2).Build()
            };

            Db.Setup(d => d.HealthChecks).ReturnsDbSet(HealthChecks);

            Handler = new GetHealthCheckQueryHandler(Db.Object, ConfigurationProvider);
        }

        public Task<GetHealthCheckQueryResponse> Handle()
        {
            return Handler.Handle(GetHealthCheckQuery, CancellationToken.None);
        }
    }
}