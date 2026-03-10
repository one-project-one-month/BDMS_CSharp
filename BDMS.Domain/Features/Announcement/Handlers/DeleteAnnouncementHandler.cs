using BDMS.Domain.Features.Announcement.Commands;
using BDMS.Shared;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace BDMS.Domain.Features.Announcement.Handlers;

public class DeleteAnnouncementHandler : IRequestHandler<DeleteAnnouncementCommand, Result<bool>>
{
    private readonly IAnnouncementService _announcementService;

    public DeleteAnnouncementHandler(IAnnouncementService announcementService)
    {
        _announcementService = announcementService;
    }

    public async Task<Result<bool>> Handle(DeleteAnnouncementCommand request, CancellationToken cancellationToken)
    {
        return await _announcementService.DeleteAnnouncement(request.Id, cancellationToken);
    }
}
