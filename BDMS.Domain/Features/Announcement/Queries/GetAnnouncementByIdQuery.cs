using BDMS.Domain.Features.Announcement.Models;
using BDMS.Shared;
using MediatR;

namespace BDMS.Domain.Features.Announcement.Queries;

public class GetAnnouncementByIdQuery : GetAnnouncementByIdReqModel, IRequest<Result<(GetAnnouncementByIdResModel Res, GetAnnouncementByIdReqModel Req)>>
{
    public GetAnnouncementByIdQuery(int id)
    {
        Id = id;
    }
}
