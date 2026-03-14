using BDMS.Domain.Features.Announcement.Models;
using BDMS.Domain.Features.Announcement.Queries;
using BDMS.Shared;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BDMS.Domain.Features.Announcement.Handlers;

public class GetAnnouncementsHandler : IRequestHandler<GetAnnouncementsQuery, Result<List<AnnouncementListItemResModel>>>
{
    private readonly IAnnouncementService _announcementService;

    public GetAnnouncementsHandler(IAnnouncementService announcementService)
    {
        _announcementService = announcementService;
    }

    public async Task<Result<List<AnnouncementListItemResModel>>> Handle(GetAnnouncementsQuery request, CancellationToken cancellationToken)
    {
        return await _announcementService.GetAnnouncements(cancellationToken);
    }
}
