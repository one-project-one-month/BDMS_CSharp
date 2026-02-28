using BDMS.Domain.Entities;
using BDMS.Infrastructure.Data;
using MediatR;

namespace BDMS.Application.Donors.Commands;

public class CreateDonorCommandHandler : IRequestHandler<CreateDonorCommand, Guid>
{
    private readonly AppDbContext _dbContext;

    public CreateDonorCommandHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> Handle(CreateDonorCommand request, CancellationToken cancellationToken)
    {
        var donor = new BloodDonor
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            BloodType = request.BloodType,
            LastDonationDate = request.LastDonationDate
        };

        await _dbContext.BloodDonors.AddAsync(donor, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return donor.Id;
    }
}
