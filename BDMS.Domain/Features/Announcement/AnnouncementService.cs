using BDMS.Database.AppDbContextModels;
using BDMS.Domain.Features.Announcement.Models;
using BDMS.Shared;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BDMS.Domain.Features.Announcement;

public class AnnouncementService : IAnnouncementService
{
    private readonly AppDbContext _dbContext;

    public AnnouncementService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<CreateAnnouncementResModel>> CreateAnnouncement(CreateAnnouncementReqModel request, CancellationToken cancellationToken)
    {
        try
        {
            var announcement = new BDMS.Database.AppDbContextModels.Announcement
            {
                Title = request.Title,
                Content = request.Content,
                IsActive = request.IsActive,
                ExpiredAt = request.ExpiredAt,
                CreatedAt = DateOnly.FromDateTime(DateTime.UtcNow),
                UpdatedAt = DateOnly.FromDateTime(DateTime.UtcNow)
            };

            _dbContext.Announcements.Add(announcement);
            await _dbContext.SaveChangesAsync(cancellationToken);

            var res = new CreateAnnouncementResModel
            {
                Id = announcement.Id,
                Title = announcement.Title,
                Content = announcement.Content,
                IsActive = announcement.IsActive,
                ExpiredAt = announcement.ExpiredAt,
                CreatedAt = announcement.CreatedAt
            };

            return Result<CreateAnnouncementResModel>.Success(res, "Announcement created successfully.");
        }
        catch (Exception ex)
        {
            return Result<CreateAnnouncementResModel>.SystemError($"An error occurred while creating the announcement: {ex.Message}");
        }
    }

    public async Task<Result<UpdateAnnouncementResModel>> UpdateAnnouncement(UpdateAnnouncementReqModel request, CancellationToken cancellationToken)
    {
        try
        {
            var announcement = await _dbContext.Announcements.FindAsync(new object[] { request.Id }, cancellationToken);
            if (announcement == null)
            {
                return Result<UpdateAnnouncementResModel>.ValidationError("Announcement not found.");
            }

            announcement.Title = request.Title;
            announcement.Content = request.Content;
            announcement.IsActive = request.IsActive;
            announcement.ExpiredAt = request.ExpiredAt;
            announcement.UpdatedAt = DateOnly.FromDateTime(DateTime.UtcNow);

            await _dbContext.SaveChangesAsync(cancellationToken);

            var res = new UpdateAnnouncementResModel
            {
                Id = announcement.Id,
                Title = announcement.Title,
                Content = announcement.Content,
                IsActive = announcement.IsActive,
                ExpiredAt = announcement.ExpiredAt,
                UpdatedAt = announcement.UpdatedAt
            };

            return Result<UpdateAnnouncementResModel>.Success(res, "Announcement updated successfully.");
        }
        catch (Exception ex)
        {
            return Result<UpdateAnnouncementResModel>.SystemError($"An error occurred while updating the announcement: {ex.Message}");
        }
    }

    public async Task<Result<DeleteAnnouncementResModel>> DeleteAnnouncement(DeleteAnnouncementReqModel request, CancellationToken cancellationToken)
    {
        try
        {
            var announcement = await _dbContext.Announcements.FindAsync(new object[] { request.Id }, cancellationToken);
            if (announcement == null)
            {
                return Result<DeleteAnnouncementResModel>.ValidationError("Announcement not found.");
            }

            _dbContext.Announcements.Remove(announcement);
            await _dbContext.SaveChangesAsync(cancellationToken);

            var res = new DeleteAnnouncementResModel
            {
                Id = request.Id,
                Message = "Announcement deleted successfully."
            };

            return Result<DeleteAnnouncementResModel>.Success(res, "Announcement deleted successfully.");
        }
        catch (Exception ex)
        {
            return Result<DeleteAnnouncementResModel>.SystemError($"An error occurred while deleting the announcement: {ex.Message}");
        }
    }

    public async Task<Result<GetAnnouncementByIdResModel>> GetAnnouncementById(GetAnnouncementByIdReqModel request, CancellationToken cancellationToken)
    {
        try
        {
            var announcement = await _dbContext.Announcements.FindAsync(new object[] { request.Id }, cancellationToken);
            if (announcement == null)
            {
                return Result<GetAnnouncementByIdResModel>.ValidationError("Announcement not found.");
            }

            var res = new GetAnnouncementByIdResModel
            {
                Id = announcement.Id,
                Title = announcement.Title,
                Content = announcement.Content,
                IsActive = announcement.IsActive,
                ExpiredAt = announcement.ExpiredAt,
                CreatedAt = announcement.CreatedAt,
                UpdatedAt = announcement.UpdatedAt
            };

            return Result<GetAnnouncementByIdResModel>.Success(res);
        }
        catch (Exception ex)
        {
            return Result<GetAnnouncementByIdResModel>.SystemError($"An error occurred while getting the announcement: {ex.Message}");
        }
    }

    public async Task<Result<List<AnnouncementListItemResModel>>> GetAnnouncements(CancellationToken cancellationToken)
    {
        try
        {
            var announcements = await _dbContext.Announcements
                .Select(a => new AnnouncementListItemResModel
                {
                    Id = a.Id,
                    Title = a.Title,
                    IsActive = a.IsActive,
                    CreatedAt = a.CreatedAt
                })
                .ToListAsync(cancellationToken);

            return Result<List<AnnouncementListItemResModel>>.Success(announcements);
        }
        catch (Exception ex)
        {
            return Result<List<AnnouncementListItemResModel>>.SystemError($"An error occurred while getting announcements: {ex.Message}");
        }
    }
}
