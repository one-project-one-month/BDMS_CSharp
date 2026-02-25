using BDMS.Domain.Features.User.Models;
using BDMS.Shared;
using MediatR;

namespace BDMS.Domain.Features.User.Queries;

public class GetAllUserQuery : IRequest<Result<List<UserRespModel>>>
{
}

