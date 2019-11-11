﻿using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using SFA.DAS.EmployerAccounts.Data;
using SFA.DAS.EmployerAccounts.Dtos;
using SFA.DAS.EmployerAccounts.Models;
using SFA.DAS.EmployerAccounts.Models.EmployerAgreement;
using SFA.DAS.HashingService;
using SFA.DAS.Validation;

namespace SFA.DAS.EmployerAccounts.Queries.GetEmployerAgreement
{
    public class GetEmployerAgreementQueryHandler : IAsyncRequestHandler<GetEmployerAgreementRequest, GetEmployerAgreementResponse>
    {
        private readonly Lazy<EmployerAccountsDbContext> _database;
        private readonly IHashingService _hashingService;
        private readonly IValidator<GetEmployerAgreementRequest> _validator;
        private readonly IConfigurationProvider _configurationProvider;
        
        public GetEmployerAgreementQueryHandler(
            Lazy<EmployerAccountsDbContext> database,
            IHashingService hashingService,
            IValidator<GetEmployerAgreementRequest> validator,
            IConfigurationProvider configurationProvider)
        {
            _database = database;
            _hashingService = hashingService;
            _validator = validator;
            _configurationProvider = configurationProvider;
        }

        public async Task<GetEmployerAgreementResponse> Handle(GetEmployerAgreementRequest message)
        {
            var validationResult = await _validator.ValidateAsync(message);

            if (!validationResult.IsValid())
            {
                throw new InvalidRequestException(validationResult.ValidationDictionary);
            }

            var agreementId = _hashingService.DecodeValue(message.AgreementId);
            var employerAgreement = await _database.Value.Agreements.ProjectTo<AgreementDto>(_configurationProvider)
                                                              .SingleOrDefaultAsync(x => x.Id.Equals(agreementId));

            if (employerAgreement == null)
                return new GetEmployerAgreementResponse();

            employerAgreement.HashedAccountId = _hashingService.HashValue(employerAgreement.AccountId);
            employerAgreement.HashedAgreementId = _hashingService.HashValue(employerAgreement.Id);
            employerAgreement.HashedLegalEntityId = _hashingService.HashValue(employerAgreement.LegalEntityId);

            if (!await UserIsAuthorizedToSignUnsignedAgreement(employerAgreement, message))
            {
                throw new UnauthorizedAccessException();
            }

            if (employerAgreement.StatusId != EmployerAgreementStatus.Signed)
            {
                employerAgreement.SignedByName = GetUserFullName(message.ExternalUserId);
            }

            return new GetEmployerAgreementResponse
            {
                EmployerAgreement = employerAgreement
            };
        }

        private async Task<bool> UserIsAuthorizedToSignUnsignedAgreement(AgreementDto employerAgreement, GetEmployerAgreementRequest message)
        {
            var caller = await _database.Value.Memberships.Where(x => x.AccountId == employerAgreement.AccountId && x.User.Ref == Guid.Parse(message.ExternalUserId)).SingleOrDefaultAsync();
            
            if (caller == null)
            {
                return false;
            }

            if (employerAgreement.HashedAccountId != message.HashedAccountId || (employerAgreement.StatusId != EmployerAgreementStatus.Signed && caller.Role != Role.Owner))
            {
                return false;
            }

            return true;
        }

        private string GetUserFullName(string userId)
        {
            var externalUserId = Guid.Parse(userId);
            var user = _database.Value.Users
                .Where(m => m.Ref == externalUserId)
                .Select(m => m)
                .Single();
            return user.FullName;
        }
    }
}