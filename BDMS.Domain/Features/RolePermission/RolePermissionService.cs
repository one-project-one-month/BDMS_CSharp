using BDMS.Database.AppDbContextModels;
using BDMS.Domain.Features.RolePermission.Model;
using BDMS.Shared;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDMS.Domain.Features.RolePermission
{
    public class RolePermissionService : IRolePermissionService
    {
        private readonly AppDbContext _db;

        public RolePermissionService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<Result<RolePermissionReqRespModel>> CreateRolePermission(RolePermissionReqRespModel rolePermissionReqRespModel)
        {
            try
            {
                bool isExists = await _db.RolePermissions.AnyAsync(
                    row => row.RoleId == rolePermissionReqRespModel.RoleId && row.PermissionId == rolePermissionReqRespModel.PermissionId);

                if (isExists)
                {
                    return Result<RolePermissionReqRespModel>.ValidationError("Role-Permission already exists.");
                }

                var rolePermission = new RolePermissionReqRespModel { RoleId = rolePermissionReqRespModel.RoleId, PermissionId = rolePermissionReqRespModel.PermissionId };

                await _db.AddAsync(rolePermission);
                await _db.SaveChangesAsync();

                var result = new RolePermissionReqRespModel { RoleId = rolePermission.RoleId, PermissionId = rolePermission.PermissionId };

                return Result<RolePermissionReqRespModel>.Success(result);
            }
            catch
            {
                return Result<RolePermissionReqRespModel>.SystemError("Error creating role-permission");
            }
        }

        public async Task<Result<RolePermissionReqRespModel>> DeleteRolePermission(RolePermissionReqRespModel model)
        {
            try
            {
                var rolePermission = await _db.RolePermissions
                    .FirstOrDefaultAsync(rp => rp.RoleId == model.RoleId && rp.PermissionId == model.PermissionId);

                if (rolePermission == null)
                {
                    return Result<RolePermissionReqRespModel>.NotFound("Role-Permission not found.");
                }

                _db.RolePermissions.Remove(rolePermission);
                await _db.SaveChangesAsync();

                var result = new RolePermissionReqRespModel { RoleId = rolePermission.RoleId, PermissionId = rolePermission.PermissionId };

                return Result<RolePermissionReqRespModel>.Success(result, "Role-Permission is deleted.");
            }
            catch
            {
                return Result<RolePermissionReqRespModel>.SystemError("Error retrieving role-permission.");
            }
        }

        public async Task<Result<List<RolePermissionReqRespModel>>> GetAllRolePermissions()
        {
            try
            {
                var results = await _db.RolePermissions
                    .AsNoTracking()
                    .Select(row => new RolePermissionReqRespModel
                    {
                        RoleId = row.RoleId,
                        PermissionId = row.PermissionId,
                    }).ToListAsync();

                return Result<List<RolePermissionReqRespModel>>.Success(results);
            }
            catch
            {
                return Result<List<RolePermissionReqRespModel>>.SystemError("Error retrieving role-permissions");
            }
        }

        public async Task<Result<RolePermissionReqRespModel>> UpdateRolePermission(RolePermissionReqRespModel oldModel, RolePermissionReqRespModel newModel)
        {
            try
            {
                var rolePermission = await _db.RolePermissions.FirstOrDefaultAsync(row => row.RoleId == oldModel.RoleId && row.PermissionId == oldModel.PermissionId );

                if (rolePermission == null)
                {
                    return Result<RolePermissionReqRespModel>.ValidationError("Role-Permission does not exist.");
                }


                bool isExists = await _db.RolePermissions.AnyAsync(
                    row => row.RoleId == newModel.RoleId && row.PermissionId == newModel.PermissionId);

                if (isExists)
                {
                    return Result<RolePermissionReqRespModel>.ValidationError("Updated Role-Permission already exists.");
                }

                _db.RolePermissions.Remove(rolePermission);
                await _db.SaveChangesAsync();

                var newRolePermission = new RolePermissionReqRespModel { RoleId = newModel.RoleId, PermissionId = newModel.PermissionId};

                await _db.AddAsync(newRolePermission);
                await _db.SaveChangesAsync();

                return Result<RolePermissionReqRespModel>.Success(newRolePermission);
            }
            catch
            {
                return Result<RolePermissionReqRespModel>.SystemError("Error updating role-permission");
            }
        }
    }
}
