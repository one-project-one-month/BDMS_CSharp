using BDMS.Domain.Features.Announcement.Models;
using BDMS.Shared;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BDMS.Domain.Features.Announcement;

public interface IAnnouncementService
{
    Task<Result<AnnouncementRespModel>> CreateAnnouncement(CreateAnnouncementRequest request, CancellationToken cancellationToken);
    Task<Result<AnnouncementRespModel>> UpdateAnnouncement(UpdateAnnouncementRequest request, CancellationToken cancellationToken);
    Task<Result<AnnouncementRespModel>> DeleteAnnouncement(DeleteAnnouncementRequest request, CancellationToken cancellationToken);
    Task<Result<AnnouncementRespModel>> GetAnnouncementById(GetAnnouncementByIdRequest request, CancellationToken cancellationToken);
    Task<Result<List<AnnouncementRespModel>>> GetAnnouncements(string? category, CancellationToken cancellationToken);
}
