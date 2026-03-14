using BDMS.Domain.Features.Announcement.Commands;
using BDMS.Domain.Features.Announcement.Models;
using BDMS.Shared;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace BDMS.Domain.Features.Announcement.Handlers;

public class DeleteAnnouncementHandler : IRequestHandler<DeleteAnnouncementCommand, Result<DeleteAnnouncementResModel>>
{
    private readonly IAnnouncementService _announcementService;

    public DeleteAnnouncementHandler(IAnnouncementService announcementService)
    {
        _announcementService = announcementService;
    }

    public async Task<Result<DeleteAnnouncementResModel>> Handle(DeleteAnnouncementCommand request, CancellationToken cancellationToken)
    {
        return await _announcementService.DeleteAnnouncement(request, cancellationToken);
    }
}
