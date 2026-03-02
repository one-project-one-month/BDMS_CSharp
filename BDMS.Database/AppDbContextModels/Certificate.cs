using System;
using System.Collections.Generic;

namespace BDMS.Database.AppDbContextModels;

public partial class Certificate
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string? CertificateTitle { get; set; }

    public string? CertificateDescription { get; set; }

    public string? CertificateData { get; set; }

    public DateOnly CreatedAt { get; set; }

    public DateOnly UpdatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
