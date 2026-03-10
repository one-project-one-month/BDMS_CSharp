using BDMS.Domain.Features.Announcement.Models;
using BDMS.Shared;
using MediatR;

namespace BDMS.Domain.Features.Announcement.Queries;

public class GetAnnouncementByIdQuery : IRequest<Result<GetAnnouncementResModel>>
{
    public int Id { get; set; }

    public GetAnnouncementByIdQuery(int id)
    {
        Id = id;
    }
}
