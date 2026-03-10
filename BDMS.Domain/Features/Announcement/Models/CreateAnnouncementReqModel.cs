using System;

namespace BDMS.Domain.Features.Announcement.Models;

public class CreateAnnouncementReqModel
{
    public string Title { get; set; } = null!;
    public string? Content { get; set; }
    public bool IsActive { get; set; }
    public DateOnly? ExpiredAt { get; set; }
}
