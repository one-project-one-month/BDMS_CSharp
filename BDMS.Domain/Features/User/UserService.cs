using BDMS.Database.AppDbContextModels;
using BDMS.Domain.Features.User.Models;
using BDMS.Shared;
using Microsoft.EntityFrameworkCore;


namespace BDMS.Domain.Features.User
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _appDbContext;

        public UserService(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<Result<UserRespModel>> DeleteUserByParameter(UserReqModel model)
        {
            try
            {
                var user = _appDbContext.Users.FirstOrDefault(row => row.Id == model.UserId || row.UserName == model.Username);
                if (user == null)
                {
                    return Result<UserRespModel>.NotFound("User not found!");
                }
                user.IsActive = false;
                user.DeletedAt = DateTime.UtcNow;
                await _appDbContext.SaveChangesAsync();

                return Result<UserRespModel>.Success(new UserRespModel(), "User deleted.");
            }
            catch (Exception ex) 
            {
                return Result<UserRespModel>.SystemError($"Error on deleting user. Error Message: {ex.Message}. ");
            }
        }

        public async Task<Result<List<UserRespModel>>> GetAllUser()
        {
            try
            {
                var result = await _appDbContext.Users
                    .AsNoTracking()
                    .Where(row => row.IsActive)
                    .Select(row => new UserRespModel
                {
                   UserId = row.Id,
                   UserRoleId = row.RoleId,
                   UserHospitalId = row.HospitalId?? 0,
                   Username = row.UserName,
                   Email = row.Email,

                }).ToListAsync();

                if (result.Count == 0) 
                {
                    return Result<List<UserRespModel>>.NotFound("No user found.");
                }

                return Result<List<UserRespModel>>.Success(result, "Success");
            }
            catch (Exception ex)
            {
                return Result<List<UserRespModel>>.SystemError($"Error retrieving User: {ex.Message}");
            }
        }

        public async Task<Result<UserRespModel>> GetUserByParameter(UserReqModel model)
        {
            try
            {
                var data = await _appDbContext.Users.AsNoTracking().FirstOrDefaultAsync(row => row.IsActive && (row.UserName == model.Username || row.Id == model.UserId));

                if (data == null)
                {
                    return Result<UserRespModel>.NotFound("No user found.");
                }
                var result = new UserRespModel { UserId = data.Id, UserRoleId = data.RoleId, UserHospitalId = data.HospitalId ?? 0, Email = data.Email , Username = data.UserName};
                return Result<UserRespModel>.Success(result, "Success");
            }
            catch (Exception ex)
            {
                return Result<UserRespModel>.SystemError($"Error retrieving User: {ex.Message}");
            }
        }

        public async Task<Result<UserRespModel>> UpdateUserByParameter(UserReqModel model)
        {
            try
            {
                var data = await _appDbContext.Users.FirstOrDefaultAsync(row => row.IsActive && (row.UserName == model.Username || row.Id == model.UserId));

                if (data == null)
                    return Result<UserRespModel>.NotFound("Cannot find the User to be updated");

                if (model.UserHospitalId != 0)
                {
                    bool hospitalExists = await _appDbContext.Hospitals.AnyAsync(hospital => hospital.Id == model.UserHospitalId);
                    if (!hospitalExists)
                    {
                        return Result<UserRespModel>.NotFound("Hospital not found.");
                    }
                }

                bool roleExists = await _appDbContext.Roles.AnyAsync(role => role.Id == model.UserRoleId);
                if (!roleExists)
                {
                    return Result<UserRespModel>.NotFound("User Role not found.");
                }

                bool emailExists = await _appDbContext.Users.AnyAsync(row => row.Email == model.Email);
                if (!emailExists)
                {
                    return Result<UserRespModel>.ValidationError("Email already exists.");
                }

                data.HospitalId = model.UserHospitalId;
                data.RoleId = model.UserRoleId;
                data.Email = model.Email??"";
                data.UpdatedAt = DateTime.UtcNow;

                var updated = await _appDbContext.SaveChangesAsync();

                var result = new UserRespModel
                {
                    UserId = data.Id,
                    Username = data.UserName,
                    Email = data.Email,
                    UserRoleId = data.RoleId,
                    UserHospitalId = data.HospitalId??0,
                };


                return Result<UserRespModel>.Success(result, "User updated successfully");
            }
            catch (Exception ex)
            {
                return Result<UserRespModel>.SystemError($"User Update failed! Error Message: {ex.Message} ");
            }
        }

        public async Task<Result<UserRespModel>> CreateUserByParameter(UserReqModel model)
        {
            try
            {
                bool userExists = await _appDbContext.Users.AnyAsync(row => row.UserName == model.Username || row.Email == model.Email);

                if (userExists)
                    return Result<UserRespModel>.ValidationError("Username or Email already exists.");

                if (model.UserHospitalId != 0)
                {
                    bool hospitalExists = await _appDbContext.Hospitals.AnyAsync(hospital => hospital.Id == model.UserHospitalId);
                    if (!hospitalExists)
                    {
                        return Result<UserRespModel>.NotFound("Hospital not found.");
                    }
                }

                bool roleExists = await _appDbContext.Roles.AnyAsync(role => role.Id == model.UserRoleId);
                if (!roleExists)
                {
                    return Result<UserRespModel>.NotFound("User Role not found.");
                }

                var data = new UserReqModel
                {
                    Username = model.Username,
                    Email = model.Email,
                    UserRoleId = model.UserRoleId,
                    UserHospitalId = model.UserHospitalId,
                };

                await _appDbContext.AddAsync(data);
                await _appDbContext.SaveChangesAsync();

                var result = new UserRespModel { UserId= data.UserId, UserRoleId = data.UserRoleId, UserHospitalId= data.UserHospitalId , Email = data.Email, Username = data.Username};

                return Result<UserRespModel>.Success(result, "User updated successfully");
            }
            catch (Exception ex)
            {
                return Result<UserRespModel>.SystemError($"User Update failed! Error Message: {ex.Message} ");
            }
        }
    }
}
