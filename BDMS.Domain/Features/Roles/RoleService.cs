using BDMS.Database.AppDbContextModels;
using BDMS.Domain.Features.Roles.Models;
using BDMS.Shared;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDMS.Domain.Features.Roles
{
    public class RoleService : IRoleService
    {
        private readonly AppDbContext _db;

        public RoleService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<Result<RolesReqRespModel>> CreateRole(RolesReqRespModel rolesReqRespModel)
        {
            try
            {
                bool isExists = await _db.Roles.AnyAsync(row => row.Name == rolesReqRespModel.Name);

                if (isExists)
                {
                    return Result<RolesReqRespModel>.ValidationError("Role already exists.");
                }

                // must add the mapped entity, not the DTO — AddAsync(RolesReqRespModel) always throws
                var role = new Role { Name = rolesReqRespModel.Name!, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };

                await _db.AddAsync(role);
                await _db.SaveChangesAsync();

                if (rolesReqRespModel.PermissionIds != null && rolesReqRespModel.PermissionIds.Count > 0)
                {
                    var rolePermissions = rolesReqRespModel.PermissionIds.Distinct().Select(pid => new BDMS.Database.AppDbContextModels.RolePermission
                    {
                        RoleId = role.Id,
                        PermissionId = pid,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    }).ToList();

                    await _db.RolePermissions.AddRangeAsync(rolePermissions);
                    await _db.SaveChangesAsync();
                }

                var result = new RolesReqRespModel { Id = role.Id, Name = role.Name, PermissionIds = rolesReqRespModel.PermissionIds };

                return Result<RolesReqRespModel>.Success(result);
            }
            catch
            {
                return Result<RolesReqRespModel>.SystemError("Error creating role");
            }
        }

        public async Task<Result<List<RolesReqRespModel>>> GetAllRoles()
        {
            try
            {
                var results = await _db.Roles
                    .AsNoTracking()
                    .Select(row => new RolesReqRespModel
                    {
                        Id = row.Id,
                        Name = row.Name,
                    }).ToListAsync();

                return Result<List<RolesReqRespModel>>.Success(results);
            }
            catch
            {
                return Result<List<RolesReqRespModel>>.SystemError("Error retrieving roles");
            }
        }

        public async Task<Result<RolesReqRespModel>> UpdateRole(RolesReqRespModel rolesReqRespModel)
        {
            try
            {
                var role = await _db.Roles.FirstOrDefaultAsync(row => row.Id == rolesReqRespModel.Id);

                if (role == null)
                {
                    return Result<RolesReqRespModel>.ValidationError("Role does not exist.");
                }

                if (string.IsNullOrWhiteSpace(rolesReqRespModel.Name))
                    return Result<RolesReqRespModel>.ValidationError("Role name should not empty.");

                role.Name = rolesReqRespModel.Name;

                if (rolesReqRespModel.PermissionIds != null)
                {
                    var existingRolePerms = await _db.RolePermissions.Where(rp => rp.RoleId == role.Id).ToListAsync();
                    var existingPermIds = existingRolePerms.Select(rp => rp.PermissionId).ToHashSet();
                    var targetPermIds = rolesReqRespModel.PermissionIds.Distinct().ToHashSet();

                    var toRemove = existingRolePerms.Where(rp => !targetPermIds.Contains(rp.PermissionId)).ToList();
                    if (toRemove.Count > 0)
                    {
                        _db.RolePermissions.RemoveRange(toRemove);
                    }

                    var toAdd = targetPermIds.Where(pid => !existingPermIds.Contains(pid))
                        .Select(pid => new BDMS.Database.AppDbContextModels.RolePermission
                        {
                            RoleId = role.Id,
                            PermissionId = pid,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        }).ToList();

                    if (toAdd.Count > 0)
                    {
                        await _db.RolePermissions.AddRangeAsync(toAdd);
                    }
                }

                await _db.SaveChangesAsync();

                var result = new RolesReqRespModel { Id = role.Id, Name = role.Name, PermissionIds = rolesReqRespModel.PermissionIds };

                return Result<RolesReqRespModel>.Success(result);
            }
            catch
            {
                return Result<RolesReqRespModel>.SystemError("Error updating role.");
            }
        }

        public async Task<Result<RolesReqRespModel>> DeleteRoleById(int Id)
        {
            try
            {
                var role = await _db.Roles
                    .FirstOrDefaultAsync(row => row.Id == Id);

                if (role == null)
                {
                    return Result<RolesReqRespModel>.NotFound("Role not found.");
                }

                bool isUsed = await _db.RolePermissions.AnyAsync(rp => rp.RoleId == Id);

                if (isUsed)
                {
                    return Result<RolesReqRespModel>.ValidationError("Role is assigned to a role.");
                }

                _db.Roles.Remove(role);
                await _db.SaveChangesAsync();

                var result = new RolesReqRespModel { Id = Id, Name = role.Name };

                return Result<RolesReqRespModel>.Success(result, "Role is deleted.");
            }
            catch
            {
                return Result<RolesReqRespModel>.SystemError("Error deleting role.");
            }
        }      
    }
}
