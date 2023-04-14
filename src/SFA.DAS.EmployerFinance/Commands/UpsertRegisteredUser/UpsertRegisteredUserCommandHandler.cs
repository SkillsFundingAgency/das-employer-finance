using System.ComponentModel.DataAnnotations;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Models.UserProfile;
using SFA.DAS.EmployerFinance.Validation;

namespace SFA.DAS.EmployerFinance.Commands.UpsertRegisteredUser;

public class UpsertRegisteredUserCommandHandler : IRequestHandler<UpsertRegisteredUserCommand, Unit>
{
    private readonly IValidator<UpsertRegisteredUserCommand> _validator;
    private readonly ILogger<UpsertRegisteredUserCommandHandler> _logger;
    private readonly IUserRepository _userRepository;

    public UpsertRegisteredUserCommandHandler(
        IValidator<UpsertRegisteredUserCommand> validator,
        ILogger<UpsertRegisteredUserCommandHandler> logger,
        IUserRepository userRepository)
    {
        _validator = validator;
        _logger = logger;
        _userRepository = userRepository;
    }

    public async Task<Unit> Handle(UpsertRegisteredUserCommand request,CancellationToken cancellationToken)
    {
        var validationResult = _validator.Validate(request);

        if (!validationResult.IsValid())
        {
            throw new ValidationException(validationResult.ConvertToDataAnnotationsValidationResult(), null, null);
        }

        await _userRepository.Upsert(new User
        {
            Ref = new Guid(request.UserRef),
            Email = request.EmailAddress,
            FirstName = request.FirstName,
            LastName = request.LastName,
            CorrelationId = request.CorrelationId
        });

        _logger.LogInformation("Upserted user with email={EmailAddress}, userRef={UserRef}, lastName={LastName}, firstName={FirstName}", request.EmailAddress, request.UserRef, request.LastName);

        return Unit.Value;
    }
}