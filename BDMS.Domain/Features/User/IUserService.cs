using BDMS.Domain.Features.User.Models;
using BDMS.Shared;

namespace BDMS.Domain.Features.User
{
    public interface IUserService
    {
        Task<Result<List<UserRespModel>>> GetAllUser();
        Task<Result<UserRespModel>> GetUserByParameter(UserReqModel model);
        Task<Result<UserRespModel>> UpdateUserByParameter(UserReqModel model);
        Task<Result<UserRespModel>> DeleteUserByParameter(UserReqModel model);
        Task<Result<UserRespModel>> CreateUserByParameter(UserReqModel model);
    }
}