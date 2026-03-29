using BDMS.Domain.Features.Announcement.Models;
using BDMS.Shared;
using MediatR;

namespace BDMS.Domain.Features.Announcement.Commands;

public class CreateAnnouncementCommand : CreateAnnouncementRequest, IRequest<Result<AnnouncementRespModel>>
{
}
