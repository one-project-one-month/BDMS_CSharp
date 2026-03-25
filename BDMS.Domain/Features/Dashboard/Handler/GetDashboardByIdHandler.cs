using BDMS.Domain.Features.Dashboard.Query;
using BDMS.Shared;
using MediatR;

namespace BDMS.Domain.Features.Dashboard.Handler
{
    public class GetDashboardByIdHandler : IRequestHandler<GetDashboardByIdQuery, Result<string>>
    {
        private readonly IDashboardService _dashboardService;
        public GetDashboardByIdHandler(IDashboardService service) 
        {
            _dashboardService = service;
        }

        public async Task<Result<string>> Handle(GetDashboardByIdQuery request, CancellationToken cancellationToken)
        {
            return await _dashboardService.GetDashboardById(request.user_Id, cancellationToken);
        }
    }
}
