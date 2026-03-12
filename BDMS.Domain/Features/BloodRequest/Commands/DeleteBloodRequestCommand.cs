using BDMS.Shared;
using MediatR;

namespace BDMS.Domain.Features.BloodRequest.Commands;

public class DeleteBloodRequestCommand : IRequest<Result<string>>
{
    public int Id { get; set; }
}
