﻿using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerFinance.Api.Types;
using SFA.DAS.EmployerFinance.Queries.GetLevyDeclaration;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerFinance.Api.UnitTests.Controllers.AccountLevyControllerTests
{
    [TestFixture]
    public class WhenIGetLevyForAnAccount : AccountLevyControllerTests
    {
        [Test]
        public async Task ThenTheLevyIsReturned()
        {
            //Arrange
            var hashedAccountId = "ABC123";
            var levyResponse = new GetLevyDeclarationResponse { Declarations = LevyDeclarationItems.Create(12334, "abc123") };
            Mediator.Setup(x => x.Send(It.Is<GetLevyDeclarationRequest>(q => q.HashedAccountId == hashedAccountId), It.IsAny<CancellationToken>())).ReturnsAsync(levyResponse);           

            //Act
            var response = await Controller.Index(hashedAccountId);

            //Assert
            Assert.IsNotNull(response);
            Assert.IsInstanceOf<OkObjectResult>(response);
            var model = ((OkObjectResult)response).Value as List<LevyDeclaration>;

            model?.Should().NotBeNull();
            Assert.IsTrue(model?.TrueForAll(x => x.HashedAccountId == hashedAccountId));
            model?.ShouldAllBeEquivalentTo(levyResponse.Declarations, options => options.Excluding(x => x.HashedAccountId).Excluding(x => x.PayeSchemeReference));
            Assert.IsTrue(model? [0].PayeSchemeReference == levyResponse.Declarations[0].EmpRef);
        }
    }
}
