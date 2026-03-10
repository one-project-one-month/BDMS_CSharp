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
    Task<Result<bool>> DeleteAnnouncement(int id, CancellationToken cancellationToken);
    Task<Result<GetAnnouncementResModel>> GetAnnouncementById(int id, CancellationToken cancellationToken);
    Task<Result<List<GetAnnouncementResModel>>> GetAnnouncements(CancellationToken cancellationToken);
}
