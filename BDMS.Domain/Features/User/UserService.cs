using BDMS.Database.AppDbContextModels;
using BDMS.Domain.Features.User.Models;
using BDMS.Shared;
using Microsoft.EntityFrameworkCore;


namespace BDMS.Domain.Features.User
{
    public class UserService
    {
        private readonly AppDbContext _appDbContext;

        public UserService(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<Result<List<UserRespModel>>> GetAllUser()
        {
            try
            {
                var result = await _appDbContext.TblEmployees.ToListAsync();
                if (result.Count == 0)
                    return Result<List<UserRespModel>>.NotFound("Cannot find the user");

                var data = result.Select(a => new UserRespModel
                {
                    UserId = a.EmployeeId,
                    Username = a.Username,
                    Email = a.Email,
                    PhoneNo = a.PhoneNo

                }).ToList();
                return Result<List<UserRespModel>>.Success(data, "Success");
            }
            catch (Exception ex)
            {
                return Result<List<UserRespModel>>.SystemError($"Error retrieving User: {ex.Message}");
            }
        }


        public async Task<Result<UserRespModel>> UpdateUser(string UserId, string PhoneNo)
        {
            try
            {
                var existingUser = await _appDbContext.TblEmployees
                    .FirstOrDefaultAsync(e => e.EmployeeId == UserId && e.DeleteFlag != true);

                if (existingUser == null)
                    return Result<UserRespModel>.NotFound("Cannot find the User to be updated");

                existingUser.PhoneNo = PhoneNo;

                var updated = await _appDbContext.SaveChangesAsync();

                var result = new UserRespModel
                {
                    UserId = UserId,
                    Username = existingUser.Username,
                    Email = existingUser.Email,
                    PhoneNo = PhoneNo
                };


                return Result<UserRespModel>.Success(result, "User updated successfully");
            }
            catch (Exception ex)
            {
                return Result<UserRespModel>.SystemError("User Update failed!");
            }
        }
    }
}
