using System;

namespace BDMS.Domain.Features.Announcement.Models;

public class AnnouncementListItemResModel
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public bool IsActive { get; set; }
    public DateOnly CreatedAt { get; set; }
}
