using System;
using System.Collections.Generic;

namespace BDMS.Database.AppDbContextModels;

public partial class Announcement
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string? Content { get; set; }

    public bool IsActive { get; set; }

    public DateOnly? ExpiredAt { get; set; }

    public DateOnly CreatedAt { get; set; }

    public DateOnly UpdatedAt { get; set; }
}
