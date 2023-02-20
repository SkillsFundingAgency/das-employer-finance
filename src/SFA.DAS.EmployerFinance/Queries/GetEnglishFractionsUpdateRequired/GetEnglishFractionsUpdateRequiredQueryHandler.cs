using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.Hmrc;

namespace SFA.DAS.EmployerFinance.Queries.GetEnglishFractionsUpdateRequired
{
    public class GetEnglishFractionsUpdateRequiredQueryHandler : IRequestHandler<GetEnglishFractionUpdateRequiredRequest, GetEnglishFractionUpdateRequiredResponse>
    {
        private readonly IHmrcService _hmrcService;
        private readonly IEnglishFractionRepository _englishFractionRepository;
        

        public GetEnglishFractionsUpdateRequiredQueryHandler(IHmrcService hmrcService, IEnglishFractionRepository englishFractionRepository)
        {
            _hmrcService = hmrcService;
            _englishFractionRepository = englishFractionRepository;
        }

        public async Task<GetEnglishFractionUpdateRequiredResponse> Handle(GetEnglishFractionUpdateRequiredRequest message,CancellationToken cancellationToken)
        {
            var hmrcLatestUpdateDate = await _hmrcService.GetLastEnglishFractionUpdate();
            var levyLatestUpdateDate = await _englishFractionRepository.GetLastUpdateDate();

            return new GetEnglishFractionUpdateRequiredResponse
            {
                UpdateRequired = hmrcLatestUpdateDate > levyLatestUpdateDate,
                DateCalculated = hmrcLatestUpdateDate
            };
        }
    }
}
