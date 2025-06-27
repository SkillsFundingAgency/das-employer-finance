using System.ComponentModel.DataAnnotations;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Models.UserProfile;
using SFA.DAS.EmployerFinance.Validation;

namespace SFA.DAS.EmployerFinance.Commands.UpsertRegisteredUser;

public class UpsertRegisteredUserCommandHandler(
    IValidator<UpsertRegisteredUserCommand> validator,
    ILogger<UpsertRegisteredUserCommandHandler> logger,
    IUserRepository userRepository)
    : IRequestHandler<UpsertRegisteredUserCommand>
{
    public async Task Handle(UpsertRegisteredUserCommand request,CancellationToken cancellationToken)
    {
        var validationResult = validator.Validate(request);

        if (!validationResult.IsValid())
        {
            throw new ValidationException(validationResult.ConvertToDataAnnotationsValidationResult(), null, null);
        }

        await userRepository.Upsert(new User
        {
            Ref = new Guid(request.UserRef),
            Email = request.EmailAddress,
            FirstName = request.FirstName,
            LastName = request.LastName,
            CorrelationId = request.CorrelationId
        });

        logger.LogInformation("Upserted user with userRef={UserRef}", request.UserRef);
    }
}