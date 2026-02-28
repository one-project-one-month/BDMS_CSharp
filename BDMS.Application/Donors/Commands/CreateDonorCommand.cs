using MediatR;

namespace BDMS.Application.Donors.Commands;

public class CreateDonorCommand : IRequest<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string BloodType { get; set; } = string.Empty;
    public DateTime? LastDonationDate { get; set; }
}
