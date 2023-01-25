using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.EmployerFinance.Data;
using SFA.DAS.EmployerFinance.Models.UserProfile;
using SFA.DAS.NLog.Logger;
using SFA.DAS.Validation;

namespace SFA.DAS.EmployerFinance.Commands.UpsertRegisteredUser
{
    public class UpsertRegisteredUserCommandHandler : IRequestHandler<UpsertRegisteredUserCommand, Unit>
    {
        private readonly IValidator<UpsertRegisteredUserCommand> _validator;
        private readonly ILog _logger;
        private readonly IUserRepository _userRepository;

        public UpsertRegisteredUserCommandHandler(
            IValidator<UpsertRegisteredUserCommand> validator,
            ILog logger,
            IUserRepository userRepository)
        {
            _validator = validator;
            _logger = logger;
            _userRepository = userRepository;
        }

        public async Task<Unit> Handle(UpsertRegisteredUserCommand request,CancellationToken cancellationToken)
        {
            var validationResult = _validator.Validate(request);

            if (!validationResult.IsValid()) throw new InvalidRequestException(validationResult.ValidationDictionary);

            await _userRepository.Upsert(new User
            {
                Ref = new Guid(request.UserRef),
                Email = request.EmailAddress,
                FirstName = request.FirstName,
                LastName = request.LastName,
                CorrelationId = request.CorrelationId
            });

            _logger.Info($"Upserted user with email={request.EmailAddress}, userRef={request.UserRef}, lastName={request.LastName}, firstName={request.FirstName}");

            return Unit.Value;
        }
    }
}
