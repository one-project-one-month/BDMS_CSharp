using BDMS.Domain.Features.Announcement.Models;
using BDMS.Shared;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BDMS.Domain.Features.Announcement;

public interface IAnnouncementService
{
    Task<Result<CreateAnnouncementResModel>> CreateAnnouncement(CreateAnnouncementReqModel request, CancellationToken cancellationToken);
    Task<Result<UpdateAnnouncementResModel>> UpdateAnnouncement(UpdateAnnouncementReqModel request, CancellationToken cancellationToken);
    Task<Result<DeleteAnnouncementResModel>> DeleteAnnouncement(DeleteAnnouncementReqModel request, CancellationToken cancellationToken);
    Task<Result<GetAnnouncementByIdResModel>> GetAnnouncementById(GetAnnouncementByIdReqModel request, CancellationToken cancellationToken);
    Task<Result<List<AnnouncementListItemResModel>>> GetAnnouncements(CancellationToken cancellationToken);
}
