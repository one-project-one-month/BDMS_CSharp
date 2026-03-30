using BDMS.Domain.Features.Announcement.Models;
using BDMS.Shared;
using MediatR;

namespace BDMS.Domain.Features.Announcement.Commands;

public class DeleteAnnouncementCommand : DeleteAnnouncementRequest, IRequest<Result<AnnouncementRespModel>>
{
    public DeleteAnnouncementCommand(int id)
    {
        Id = id;
    }
}
