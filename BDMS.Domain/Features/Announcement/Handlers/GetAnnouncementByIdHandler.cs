using BDMS.Domain.Features.Announcement.Models;
using BDMS.Domain.Features.Announcement.Queries;
using BDMS.Shared;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace BDMS.Domain.Features.Announcement.Handlers;

public class GetAnnouncementByIdHandler : IRequestHandler<GetAnnouncementByIdQuery, Result<GetAnnouncementResModel>>
{
    private readonly IAnnouncementService _announcementService;

    public GetAnnouncementByIdHandler(IAnnouncementService announcementService)
    {
        _announcementService = announcementService;
    }

    public async Task<Result<GetAnnouncementResModel>> Handle(GetAnnouncementByIdQuery request, CancellationToken cancellationToken)
    {
        return await _announcementService.GetAnnouncementById(request.Id, cancellationToken);
    }
}
