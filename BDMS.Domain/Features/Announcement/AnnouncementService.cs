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

    public async Task<Result<(CreateAnnouncementResModel Res, CreateAnnouncementReqModel Req)>> CreateAnnouncement(CreateAnnouncementReqModel request, CancellationToken cancellationToken)
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

            return Result<(CreateAnnouncementResModel, CreateAnnouncementReqModel)>.Success((res, request), "Announcement created successfully.");
        }
        catch (Exception ex)
        {
            return Result<(CreateAnnouncementResModel, CreateAnnouncementReqModel)>.SystemError($"An error occurred while creating the announcement: {ex.Message}");
        }
    }

    public async Task<Result<(UpdateAnnouncementResModel Res, UpdateAnnouncementReqModel Req)>> UpdateAnnouncement(UpdateAnnouncementReqModel request, CancellationToken cancellationToken)
    {
        try
        {
            var announcement = await _dbContext.Announcements.FindAsync(new object[] { request.Id }, cancellationToken);
            if (announcement == null)
            {
                return Result<(UpdateAnnouncementResModel, UpdateAnnouncementReqModel)>.ValidationError("Announcement not found.");
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

            return Result<(UpdateAnnouncementResModel, UpdateAnnouncementReqModel)>.Success((res, request), "Announcement updated successfully.");
        }
        catch (Exception ex)
        {
            return Result<(UpdateAnnouncementResModel, UpdateAnnouncementReqModel)>.SystemError($"An error occurred while updating the announcement: {ex.Message}");
        }
    }

    public async Task<Result<(DeleteAnnouncementResModel Res, DeleteAnnouncementReqModel Req)>> DeleteAnnouncement(DeleteAnnouncementReqModel request, CancellationToken cancellationToken)
    {
        try
        {
            var announcement = await _dbContext.Announcements.FindAsync(new object[] { request.Id }, cancellationToken);
            if (announcement == null)
            {
                return Result<(DeleteAnnouncementResModel, DeleteAnnouncementReqModel)>.ValidationError("Announcement not found.");
            }

            _dbContext.Announcements.Remove(announcement);
            await _dbContext.SaveChangesAsync(cancellationToken);

            var res = new DeleteAnnouncementResModel
            {
                Id = request.Id,
                Message = "Announcement deleted successfully."
            };

            return Result<(DeleteAnnouncementResModel, DeleteAnnouncementReqModel)>.Success((res, request), "Announcement deleted successfully.");
        }
        catch (Exception ex)
        {
            return Result<(DeleteAnnouncementResModel, DeleteAnnouncementReqModel)>.SystemError($"An error occurred while deleting the announcement: {ex.Message}");
        }
    }

    public async Task<Result<(GetAnnouncementByIdResModel Res, GetAnnouncementByIdReqModel Req)>> GetAnnouncementById(GetAnnouncementByIdReqModel request, CancellationToken cancellationToken)
    {
        try
        {
            var announcement = await _dbContext.Announcements.FindAsync(new object[] { request.Id }, cancellationToken);
            if (announcement == null)
            {
                return Result<(GetAnnouncementByIdResModel, GetAnnouncementByIdReqModel)>.ValidationError("Announcement not found.");
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

            return Result<(GetAnnouncementByIdResModel, GetAnnouncementByIdReqModel)>.Success((res, request));
        }
        catch (Exception ex)
        {
            return Result<(GetAnnouncementByIdResModel, GetAnnouncementByIdReqModel)>.SystemError($"An error occurred while getting the announcement: {ex.Message}");
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
