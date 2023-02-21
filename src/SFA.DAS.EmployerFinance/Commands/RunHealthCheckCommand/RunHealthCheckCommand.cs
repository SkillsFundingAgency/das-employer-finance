using System.ComponentModel.DataAnnotations;
using AutoMapper.Configuration.Annotations;
using SFA.DAS.Authorization.ModelBinding;

namespace SFA.DAS.EmployerFinance.Commands.RunHealthCheckCommand;

public class RunHealthCheckCommand : IAuthorizationContextModel, IRequest<Unit>
{
    [Ignore]
    [Required]
    public Guid? UserRef { get; set; }
}