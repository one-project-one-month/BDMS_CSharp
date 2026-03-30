using BDMS.Domain.Features.Announcement.Models;
using BDMS.Shared;
using MediatR;

namespace BDMS.Domain.Features.Announcement.Queries;

public class GetAnnouncementByIdQuery : GetAnnouncementByIdRequest, IRequest<Result<AnnouncementRespModel>>
{
    public GetAnnouncementByIdQuery(int id)
    {
        Id = id;
    }
}
