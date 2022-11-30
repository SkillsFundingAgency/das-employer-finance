﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Data;
using SFA.DAS.EmployerAccounts.Infrastructure.OuterApi.OuterApiRequests.Finance;
using SFA.DAS.EmployerAccounts.Infrastructure.OuterApi.OuterApiResponses.Finance;
using SFA.DAS.EmployerAccounts.Interfaces;
using SFA.DAS.EmployerAccounts.Mappings;
using SFA.DAS.EmployerAccounts.Models.PAYE;
using SFA.DAS.EmployerAccounts.Services;
using SFA.DAS.HashingService;
using OuterApiDasEnglishFraction = SFA.DAS.EmployerAccounts.Infrastructure.OuterApi.OuterApiResponses.Finance.DasEnglishFraction;

namespace SFA.DAS.EmployerAccounts.UnitTests.Services.PayeSchemes
{
    class WhenIGetAccountPayeSchemesWithEnglishFraction
    {
        private const long AccountId = 2;
        private static readonly DateTime UpdateDate = DateTime.Now;

        private PayeView _payeView;
        private OuterApiDasEnglishFraction _englishFraction;

        private Mock<IOuterApiClient> _outerApiClient;
        private Mock<IPayeRepository> _payeSchemesRepository;
        private Mock<IHashingService> _hashingService;
        private IMapper _mapper;

        private IPayeSchemesWithEnglishFractionService _sut;
        private string _hashedAccountId;

        [SetUp]
        public void Arrange()
        {
            _payeView = new PayeView
            {
                AccountId = AccountId,
                Ref = "123/ABC"
            };

            _englishFraction = new OuterApiDasEnglishFraction
            {
                EmpRef = _payeView.Ref,
                DateCalculated = UpdateDate,
                Amount = 0.5m
            };

            _hashedAccountId = "123ABC";

            _outerApiClient = new Mock<IOuterApiClient>();
            _payeSchemesRepository = new Mock<IPayeRepository>();
            _hashingService = new Mock<IHashingService>();
            
            _mapper = new Mapper(new MapperConfiguration(c =>
            {
                c.AddProfile<LevyMappings>();
            }));

            _payeSchemesRepository.Setup(x => x.GetPayeSchemesByAccountId(It.IsAny<long>())).ReturnsAsync(new List<PayeView>
            {
                _payeView
            });

            _outerApiClient
                .Setup(s => s.Get<GetEnglishFractionCurrentResponse>(It.IsAny<GetEnglishFractionCurrentRequest>()))
                .ReturnsAsync(new GetEnglishFractionCurrentResponse { Fractions = new List<OuterApiDasEnglishFraction> { _englishFraction } });

            _hashingService.Setup(x => x.DecodeValue(It.IsAny<string>()))
                .Returns(AccountId);

            _sut = new PayeSchemesWithEnglishFractionService(
                _outerApiClient.Object,
                _payeSchemesRepository.Object,
                _hashingService.Object,
                _mapper);
        }

        [Test]
        public  async Task ThenIfAccountIdIsValidThenOuterApiIsCalled()
        {
            await _sut.GetPayeSchemes(_hashedAccountId);

            _outerApiClient.Verify(v => v.Get<GetEnglishFractionCurrentResponse>(It.IsAny<GetEnglishFractionCurrentRequest>()), Times.Once);
        }

        [Test]
        public  async Task ThenIfAccountIdIsValidPayeSchemesAreReturned()
        {
            //Act
            var result = await _sut.GetPayeSchemes(_hashedAccountId);

            //Assert
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(_payeView, result.First());
            Assert.True(CompareEnglishFractions(_englishFraction, result.First().EnglishFraction));
        }

        [Test]
        public async Task ThenIfNotSchemesCanBeFoundNoEnglishFractionsAreCollected()
        {
            _payeSchemesRepository.Setup(x => x.GetPayeSchemesByAccountId(It.IsAny<long>()))
                              .ReturnsAsync(new List<PayeView>());

            var result = await _sut.GetPayeSchemes(_hashedAccountId);

            //Assert
            Assert.IsEmpty(result);
            _outerApiClient.Verify(v => v.Get<GetEnglishFractionCurrentResponse>(It.IsAny<GetEnglishFractionCurrentRequest>()), Times.Never);
        }

        private bool CompareEnglishFractions(OuterApiDasEnglishFraction first, EmployerAccounts.Models.Levy.DasEnglishFraction second)
        {
            return JsonConvert.SerializeObject(first).Equals(JsonConvert.SerializeObject(second));
        }
        
    }
}
