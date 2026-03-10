using BDMS.Domain.Features.Announcement.Commands;
using BDMS.Domain.Features.Announcement.Models;
using BDMS.Shared;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace BDMS.Domain.Features.Announcement.Handlers;

public class UpdateAnnouncementHandler : IRequestHandler<UpdateAnnouncementCommand, Result<UpdateAnnouncementResModel>>
{
    private readonly IAnnouncementService _announcementService;

    public UpdateAnnouncementHandler(IAnnouncementService announcementService)
    {
        _announcementService = announcementService;
    }

    public async Task<Result<UpdateAnnouncementResModel>> Handle(UpdateAnnouncementCommand request, CancellationToken cancellationToken)
    {
        return await _announcementService.UpdateAnnouncement(request, cancellationToken);
    }
}
