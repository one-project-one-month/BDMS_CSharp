using BDMS.Database.AppDbContextModels;
using BDMS.Domain.Features.User.Models;
using BDMS.Shared;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;


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
                var user = _appDbContext.Users
                    .Include(u => u.Role)
                    .Include(u => u.Hospital)
                    .Include(u => u.Donor)
                    .FirstOrDefault(row => row.Id == model.UserId);
                
                if (user == null)
                {
                    return Result<UserRespModel>.NotFound("User not found!");
                }
                user.IsActive = false;

                user.DeletedAt = DateTime.UtcNow;

                if(user.Donor != null)
                {
                    user.Donor.IsActive = false;
                    user.DeletedAt = DateTime.UtcNow;
                }

                 _appDbContext.Update(user);

                await _appDbContext.SaveChangesAsync();

                return Result<UserRespModel>.DeleteSuccess("User deleted.");
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
                //var userRole = await _appDbContext.Users
                var result = await _appDbContext.Users
                    .AsNoTracking()
                    .Select(row => new UserRespModel
                {
                   UserId = row.Id,
                    //UserRoleId = row.RoleId,
                    //RoleName = row.Role.Name,
                    //UserHospitalId = row.HospitalId,
                    //HospitalName = row.Hospital != null ? row.Hospital.Name : null,
                    Role = new RoleModel { RoleId = row.RoleId, RoleName = row.Role.Name },
                    Hospital = row.Hospital != null ? new HospitalModel
                    {
                        HospitalId = row.HospitalId,
                        HospitalName = row.Hospital.Name
                    } : null,
                    Username = row.UserName,
                    IsActive = row.IsActive,
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
                var data = await _appDbContext.Users.AsNoTracking()
                    .Include(u => u.Role)
                    .Include(u => u.Hospital)
                    .FirstOrDefaultAsync(row => row.IsActive &&  row.Id == model.UserId);

                if (data == null)
                {
                    return Result<UserRespModel>.NotFound("No user found.");
                }
                var result = new UserRespModel 
                { 
                    UserId = data.Id, 
                    //UserRoleId = data.RoleId, 
                    Role = new RoleModel { RoleId = data.RoleId, RoleName = data.Role.Name},
                    Hospital = data.Hospital != null ? new HospitalModel
                    {
                        HospitalId = data.HospitalId,
                        HospitalName = data.Hospital.Name
                    } : null,
                    //UserHospitalId = data.HospitalId, 
                    Email = data.Email , 
                    IsActive = data.IsActive,
                    Username = data.UserName
                };
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
                var user = await _appDbContext.Users
                    .Include(u => u.Role)
                    .Include(u => u.Hospital)
                    .FirstOrDefaultAsync(row => row.IsActive && (row.UserName == model.Username || row.Id == model.UserId));

                if (user == null)
                    return Result<UserRespModel>.NotFound("Cannot find the User to be updated");

                if (model.UserHospitalId != 0 && model.UserHospitalId != null)
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

                bool emailTaken = await _appDbContext.Users
                                        .AnyAsync(row => row.Email == model.Email && row.Id != model.UserId);
                if (emailTaken)
                    return Result<UserRespModel>.ValidationError("Email already exists.");

                user.HospitalId = model.UserHospitalId;
                user.RoleId = model.UserRoleId;
                user.UserName = model.Username ?? "";
                user.Email = model.Email ?? "";
                user.UpdatedAt = DateTime.UtcNow;

                await _appDbContext.SaveChangesAsync();

                await _appDbContext.Entry(user).Reference(u => u.Role).LoadAsync();
                await _appDbContext.Entry(user).Reference(u => u.Hospital).LoadAsync();

                var result = new UserRespModel
                {
                    UserId = user.Id,
                    Username = user.UserName,
                    Email = user.Email,
                    //UserRoleId = data.RoleId,
                    //UserHospitalId = data.HospitalId,
                    Role = new RoleModel { RoleId = user.RoleId, RoleName = user.Role.Name },
                    Hospital = user.Hospital != null ? new HospitalModel
                    {
                        HospitalId = user.HospitalId,
                        HospitalName = user.Hospital.Name
                    } : null,
                };


                return Result<UserRespModel>.Success(result, "User updated successfully");
            }
            catch (Exception ex)
            {
                return Result<UserRespModel>.SystemError($"User Update failed! Error Message: {ex.Message} ");
            }
        }

        public async Task<Result<UserRespModel>> CreateUserByParameter(CreateUserReqModel model)
        {
            try
            {
                bool userExists = await _appDbContext.Users
                    .AnyAsync(row => row.Email == model.Email);

                if (userExists)
                    return Result<UserRespModel>.ValidationError("Username or Email already exists.");

                if (model.UserHospitalId != 0 && model.UserHospitalId != null)
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

                var user = new Database.AppDbContextModels.User
                {
                    UserName = model.Username,
                    Email = model.Email,
                    Password = model.Password.HashPassword(),
                    RoleId = model.UserRoleId,
                    HospitalId = (model.UserHospitalId == null || model.UserHospitalId == 0) ? null : model.UserHospitalId,
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                await _appDbContext.AddAsync(user);
                await _appDbContext.SaveChangesAsync();

                await _appDbContext.Entry(user).Reference(u => u.Role).LoadAsync();
                await _appDbContext.Entry(user).Reference(u => u.Hospital).LoadAsync();

                var result = new UserRespModel
                {
                    UserId = user.Id,
                    Username = user.UserName,
                    Email = user.Email,
                    //UserRoleId = user.RoleId,
                    //UserHospitalId = user.HospitalId,
                    Role = new RoleModel { RoleId = user.RoleId, RoleName = user.Role.Name },
                    Hospital = user.Hospital != null ? new HospitalModel
                    {
                        HospitalId = user.Hospital.Id,
                        HospitalName = user.Hospital.Name,
                    } : null,
                };

                return Result<UserRespModel>.Success(result, "User created successfully");
            }
            catch (Exception ex)
            {
                return Result<UserRespModel>.SystemError($"User created failed! Error Message: {ex.Message} ");
            }
        }

        public async Task<Result<UserRespModel>> UpdateUserStatus(int userId, bool isActive)
        {
            try
            {
                var user = await _appDbContext.Users
                    .Include(u => u.Donor)
                    .FirstOrDefaultAsync(u => u.Id == userId);
                if(user == null)
                {
                    return Result<UserRespModel>.NotFound("User Not Found");
                }

                user.IsActive = isActive;
                user.UpdatedAt = DateTime.Now;

                if (!isActive)
                    user.DeletedAt = DateTime.Now;
                else
                    user.DeletedAt = null;

                if(user.Donor != null)
                {
                    user.Donor.IsActive = isActive;
                    user.UpdatedAt = DateTime.Now;
                    user.DeletedAt = isActive ? DateTime.Now : null;
                }

                await _appDbContext.SaveChangesAsync();

                await _appDbContext.Entry(user).Reference(u => u.Role).LoadAsync();
                await _appDbContext.Entry(user).Reference(u => u.Hospital).LoadAsync();

                var result = new UserRespModel
                {
                    UserId = user.Id,
                    Username = user.UserName,
                    Email = user.Email,
                    //UserRoleId = user.RoleId,
                    //UserHospitalId = user.HospitalId,
                    Role = new RoleModel { RoleId = user.RoleId, RoleName = user.Role.Name },
                    Hospital = user.Hospital != null ? new HospitalModel
                    {
                        HospitalId = user.Hospital.Id,
                        HospitalName = user.Hospital.Name,
                    } : null,
                };

                return Result<UserRespModel>.Success(result, isActive ? "User activated successfully" : "User deactivated successfully");
                 

            }
            catch(Exception ex)
            {
                return Result<UserRespModel>.SystemError($"Status update failed! Error: {ex.Message}");
            }
        }
    }
}
