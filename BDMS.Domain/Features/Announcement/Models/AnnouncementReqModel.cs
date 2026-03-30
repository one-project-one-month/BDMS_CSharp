using BDMS.Shared.Enums;
using System;

namespace BDMS.Domain.Features.Announcement.Models;

public class CreateAnnouncementRequest
{
    public string Title { get; set; } = null!;
    public string? Content { get; set; }
    public EnumAnnouncementCategory Category { get; set; }
    public bool IsActive { get; set; }
    public DateOnly? ExpiredAt { get; set; }
}

public class UpdateAnnouncementRequest : CreateAnnouncementRequest
{
    public int Id { get; set; }
}

public class DeleteAnnouncementRequest
{
    public int Id { get; set; }
}

public class GetAnnouncementByIdRequest
{
    public int Id { get; set; }
}