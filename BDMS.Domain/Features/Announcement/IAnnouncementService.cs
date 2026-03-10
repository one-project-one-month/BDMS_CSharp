using BDMS.Domain.Features.Announcement.Models;
using BDMS.Shared;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BDMS.Domain.Features.Announcement;

public interface IAnnouncementService
{
    Task<Result<(CreateAnnouncementResModel Res, CreateAnnouncementReqModel Req)>> CreateAnnouncement(CreateAnnouncementReqModel request, CancellationToken cancellationToken);
    Task<Result<(UpdateAnnouncementResModel Res, UpdateAnnouncementReqModel Req)>> UpdateAnnouncement(UpdateAnnouncementReqModel request, CancellationToken cancellationToken);
    Task<Result<(DeleteAnnouncementResModel Res, DeleteAnnouncementReqModel Req)>> DeleteAnnouncement(DeleteAnnouncementReqModel request, CancellationToken cancellationToken);
    Task<Result<(GetAnnouncementByIdResModel Res, GetAnnouncementByIdReqModel Req)>> GetAnnouncementById(GetAnnouncementByIdReqModel request, CancellationToken cancellationToken);
    Task<Result<List<AnnouncementListItemResModel>>> GetAnnouncements(CancellationToken cancellationToken);
}
