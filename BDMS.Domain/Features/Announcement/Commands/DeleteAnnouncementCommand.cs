using BDMS.Domain.Features.Announcement.Models;
using BDMS.Shared;
using MediatR;

namespace BDMS.Domain.Features.Announcement.Commands;

public class DeleteAnnouncementCommand : DeleteAnnouncementReqModel, IRequest<Result<(DeleteAnnouncementResModel Res, DeleteAnnouncementReqModel Req)>>
{
    public DeleteAnnouncementCommand(int id)
    {
        Id = id;
    }
}
