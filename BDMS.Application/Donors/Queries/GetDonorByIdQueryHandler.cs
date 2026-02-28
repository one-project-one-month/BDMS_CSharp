using BDMS.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BDMS.Application.Donors.Queries;

public class GetDonorByIdQueryHandler : IRequestHandler<GetDonorByIdQuery, DonorDto?>
{
    private readonly AppDbContext _dbContext;

    public GetDonorByIdQueryHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DonorDto?> Handle(GetDonorByIdQuery request, CancellationToken cancellationToken)
    {
        var donor = await _dbContext.BloodDonors
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (donor is null)
        {
            return null;
        }

        return new DonorDto
        {
            Id = donor.Id,
            Name = donor.Name,
            BloodType = donor.BloodType,
            LastDonationDate = donor.LastDonationDate,
            CanDonate = donor.CanDonate()
        };
    }
}
