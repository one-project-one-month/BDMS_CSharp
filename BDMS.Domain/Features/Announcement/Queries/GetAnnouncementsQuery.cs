using BDMS.Domain.Features.Announcement.Models;
using BDMS.Shared;
using MediatR;
using System.Collections.Generic;

namespace BDMS.Domain.Features.Announcement.Queries;

public class GetAnnouncementsQuery : IRequest<Result<List<AnnouncementListItemResModel>>>
{
}
