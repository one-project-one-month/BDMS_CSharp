using BDMS.Shared;
using MediatR;

namespace BDMS.Domain.Features.Announcement.Commands;

public class DeleteAnnouncementCommand : IRequest<Result<bool>>
{
    public int Id { get; set; }

    public DeleteAnnouncementCommand(int id)
    {
        Id = id;
    }
}
