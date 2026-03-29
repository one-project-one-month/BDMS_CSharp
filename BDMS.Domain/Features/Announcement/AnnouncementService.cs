using BDMS.Database.AppDbContextModels;
using BDMS.Domain.Features.Announcement.Models;
using BDMS.Shared;
using BDMS.Shared.Enums;
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

    public async Task<Result<AnnouncementRespModel>> CreateAnnouncement(BDMS.Domain.Features.Announcement.Models.CreateAnnouncementRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var announcement = new BDMS.Database.AppDbContextModels.Announcement
            {
                Title = request.Title,
                Content = request.Content,
                Category = request.Category.ToString(),
                IsActive = request.IsActive,
                ExpiredAt = request.ExpiredAt,
                CreatedAt = DateOnly.FromDateTime(DateTime.UtcNow),
                UpdatedAt = DateOnly.FromDateTime(DateTime.UtcNow)
            };

            _dbContext.Announcements.Add(announcement);
            await _dbContext.SaveChangesAsync(cancellationToken);

            var res = new AnnouncementRespModel
            {
                Id = announcement.Id,
                Title = announcement.Title,
                Category = announcement.Category,
                Content = announcement.Content,
                IsActive = announcement.IsActive,
                ExpiredAt = announcement.ExpiredAt,
                CreatedAt = announcement.CreatedAt,
                UpdatedAt = announcement.UpdatedAt
            };

            return Result<AnnouncementRespModel>.Success(res, "Announcement created successfully.");
        }
        catch (Exception ex)
        {
            return Result<AnnouncementRespModel>.SystemError($"An error occurred while creating the announcement: {ex.Message}");
        }
    }

    public async Task<Result<AnnouncementRespModel>> UpdateAnnouncement(UpdateAnnouncementRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var announcement = await _dbContext.Announcements.FindAsync(new object[] { request.Id }, cancellationToken);
            if (announcement == null)
            {
                return Result<AnnouncementRespModel>.ValidationError("Announcement not found.");
            }

            announcement.Title = request.Title;
            announcement.Content = request.Content;
            announcement.Category = request.Category.ToString();
            announcement.IsActive = request.IsActive;
            announcement.ExpiredAt = request.ExpiredAt;
            announcement.UpdatedAt = DateOnly.FromDateTime(DateTime.UtcNow);

            await _dbContext.SaveChangesAsync(cancellationToken);

            var res = new AnnouncementRespModel
            {
                Id = announcement.Id,
                Title = announcement.Title,
                Category = announcement.Category,
                Content = announcement.Content,
                IsActive = announcement.IsActive,
                ExpiredAt = announcement.ExpiredAt,
                CreatedAt = announcement.CreatedAt,
                UpdatedAt = announcement.UpdatedAt
            };

            return Result<AnnouncementRespModel>.Success(res, "Announcement updated successfully.");
        }
        catch (Exception ex)
        {
            return Result<AnnouncementRespModel>.SystemError($"An error occurred while updating the announcement: {ex.Message}");
        }
    }

    public async Task<Result<AnnouncementRespModel>> DeleteAnnouncement(DeleteAnnouncementRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var announcement = await _dbContext.Announcements.FindAsync(new object[] { request.Id }, cancellationToken);
            if (announcement == null)
            {
                return Result<AnnouncementRespModel>.ValidationError("Announcement not found.");
            }

            _dbContext.Announcements.Remove(announcement);
            await _dbContext.SaveChangesAsync(cancellationToken);

            var res = new AnnouncementRespModel
            {
                Id = request.Id,
            };

            return Result<AnnouncementRespModel>.Success(res, "Announcement deleted successfully.");
        }
        catch (Exception ex)
        {
            return Result<AnnouncementRespModel>.SystemError($"An error occurred while deleting the announcement: {ex.Message}");
        }
    }

    public async Task<Result<AnnouncementRespModel>> GetAnnouncementById(GetAnnouncementByIdRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var announcement = await _dbContext.Announcements.FindAsync(new object[] { request.Id }, cancellationToken);
            if (announcement == null)
            {
                return Result<AnnouncementRespModel>.ValidationError("Announcement not found.");
            }

            var res = new AnnouncementRespModel
            {
                Id = announcement.Id,
                Title = announcement.Title,
                Category = announcement.Category,
                Content = announcement.Content,
                IsActive = announcement.IsActive,
                ExpiredAt = announcement.ExpiredAt,
                CreatedAt = announcement.CreatedAt,
                UpdatedAt = announcement.UpdatedAt
            };

            return Result<AnnouncementRespModel>.Success(res);
        }
        catch (Exception ex)
        {
            return Result<AnnouncementRespModel>.SystemError($"An error occurred while getting the announcement: {ex.Message}");
        }
    }

    public async Task<Result<List<AnnouncementRespModel>>> GetAnnouncements(string? category, CancellationToken cancellationToken)
    {
        try
        {
            var announcementsQuery = _dbContext.Announcements.AsQueryable();

            if (!string.IsNullOrEmpty(category))
            {
                announcementsQuery = announcementsQuery.Where(a => a.Category == category);
            }

            var announcements = await announcementsQuery
                .Select(a => new AnnouncementRespModel
                {
                    Id = a.Id,
                    Title = a.Title,
                    Category = a.Category,
                    Content = a.Content,
                    IsActive = a.IsActive,
                    ExpiredAt = a.ExpiredAt,
                    CreatedAt = a.CreatedAt,
                    UpdatedAt = a.UpdatedAt
                })
                .ToListAsync(cancellationToken);

            return Result<List<AnnouncementRespModel>>.Success(announcements);
        }
        catch (Exception ex)
        {
            return Result<List<AnnouncementRespModel>>.SystemError($"An error occurred while getting announcements: {ex.Message}");
        }
    }
}
