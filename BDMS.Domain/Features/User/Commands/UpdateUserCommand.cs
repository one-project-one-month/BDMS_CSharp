using BDMS.Domain.Features.User.Models;
using BDMS.Shared;
using MediatR;

namespace BDMS.Domain.Features.User.Commands;

public class UpdateUserCommand : IRequest<Result<UserRespModel>>
{
    public required int UserId { get; set; }
    public int UserRoleId { get; set; }
    public int hospital_id {  get; set; }
    public string? UserName { get; set; }
    public string? Email { get; set; }
}

