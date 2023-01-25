using System.ComponentModel.DataAnnotations;
using MediatR;

namespace SFA.DAS.EmployerFinance.Queries.GetEmployerAccountDetail
{
    public class GetEmployerAccountDetailByHashedIdQuery : IRequest<GetEmployerAccountDetailByHashedIdResponse>
    {
        [Required]
        public string HashedAccountId { get; set; }
    }
}