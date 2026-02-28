using MediatR;

namespace BDMS.Application.Donors.Queries;

public class GetDonorByIdQuery : IRequest<DonorDto?>
{
    public GetDonorByIdQuery(Guid id)
    {
        Id = id;
    }

    public Guid Id { get; }
}
