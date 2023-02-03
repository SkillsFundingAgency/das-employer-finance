using MediatR;

namespace SFA.DAS.EmployerFinance.Web.Orchestrators
{
    public class HomeOrchestrator
    {
        private readonly IMediator _mediator;

        public HomeOrchestrator() { }   

        public HomeOrchestrator(IMediator mediator)
        {
            _mediator = mediator;   
        }
    }
}
