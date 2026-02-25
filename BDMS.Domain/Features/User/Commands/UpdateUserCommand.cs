using BDMS.Domain.Features.User.Models;
using BDMS.Shared;
using MediatR;

namespace BDMS.Domain.Features.User.Commands;

public class UpdateUserCommand : IRequest<Result<UserRespModel>>
{
    public string UserId { get; set; }
    public string PhoneNo { get; set; } = null!;
}

