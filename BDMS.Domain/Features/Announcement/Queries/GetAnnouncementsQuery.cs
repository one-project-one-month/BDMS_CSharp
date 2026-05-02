using BDMS.Domain.Features.Announcement.Models;
using BDMS.Shared;
using BDMS.Shared.Enums;
using MediatR;
using System.Collections.Generic;

namespace BDMS.Domain.Features.Announcement.Queries;

public class GetAnnouncementsQuery : IRequest<Result<List<AnnouncementRespModel>>>
{
    public EnumAnnouncementCategory Category { get; set; }
}
