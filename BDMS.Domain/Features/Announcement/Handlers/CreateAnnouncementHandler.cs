using BDMS.Domain.Features.Announcement.Commands;
using BDMS.Domain.Features.Announcement.Models;
using BDMS.Shared;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace BDMS.Domain.Features.Announcement.Handlers;

public class CreateAnnouncementHandler : IRequestHandler<CreateAnnouncementCommand, Result<AnnouncementRespModel>>
{
    private readonly IAnnouncementService _announcementService;

    public CreateAnnouncementHandler(IAnnouncementService announcementService)
    {
        _announcementService = announcementService;
    }

    public async Task<Result<AnnouncementRespModel>> Handle(CreateAnnouncementCommand request, CancellationToken cancellationToken)
    {
        return await _announcementService.CreateAnnouncement(request, cancellationToken);
    }
}
