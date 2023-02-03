using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.EmployerFinance.Queries.GetTransferAllowance;
using SFA.DAS.EmployerFinance.Web.ViewModels;

namespace SFA.DAS.EmployerFinance.Web.ViewComponents
{
    public class TransferAllowanceViewComponent : ViewComponent
    {
        private readonly IMediator _mediator;
        private readonly GetTransferAllowanceQuery _query;
        private readonly IMapper _mapper;
        public TransferAllowanceViewComponent(IMediator mediator, GetTransferAllowanceQuery query, IMapper mapper)
        {
            _mediator = mediator;
            _query = query;
            _mapper= mapper;
        }

        public IViewComponentResult Invoke(TransferAllowanceViewModel model)
        {
            var response = Task.Run(() => _mediator.Send(_query)).GetAwaiter().GetResult();
            return View(_mapper.Map<TransferAllowanceViewModel>(response));
        }
    }
}
