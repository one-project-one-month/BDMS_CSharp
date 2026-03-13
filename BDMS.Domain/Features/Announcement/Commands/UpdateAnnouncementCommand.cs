using BDMS.Domain.Features.Announcement.Models;
using BDMS.Shared;
using MediatR;

namespace BDMS.Domain.Features.Announcement.Commands;

public class UpdateAnnouncementCommand : UpdateAnnouncementReqModel, IRequest<Result<UpdateAnnouncementResModel>>
{
}
