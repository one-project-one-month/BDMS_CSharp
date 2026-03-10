using BDMS.Database.AppDbContextModels;
using BDMS.Domain.Features.Permissions.Models;
using BDMS.Shared;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDMS.Domain.Features.Permissions
{
    public class PermissionService
    {
        private readonly AppDbContext _db;


        public PermissionService (AppDbContext db)
        {
            _db = db;
        }

        public async Task<Result<PermissionReqRespModel>> CreatePermission(PermissionReqRespModel permissionReqRespModel)
        {
            try
            {
                bool isExists = await _db.Permissions.AnyAsync(row => row.Name == permissionReqRespModel.Name);

                if (isExists)
                {
                    return Result<PermissionReqRespModel>.ValidationError("Permission already exists.");
                }

                var permission = new PermissionReqRespModel { Name = permissionReqRespModel.Name };

                await _db.AddAsync(permission);
                await _db.SaveChangesAsync();

                var result = new PermissionReqRespModel { Id = permission.Id, Name = permission.Name };

                return Result<PermissionReqRespModel>.Success(result);
            }
            catch (Exception ex)
            {
                return Result<PermissionReqRespModel>.SystemError("Error creating permission");
            }
        }

        public async Task<Result<List<PermissionReqRespModel>>> GetAllPermissions()
        {
            try
            {
                var results = await _db.Permissions
                    .AsNoTracking()
                    .Select(row => new PermissionReqRespModel
                    {
                        Id = row.Id,
                        Name = row.Name,
                    }).ToListAsync();

                return Result<List<PermissionReqRespModel>>.Success(results);
            }
            catch (Exception ex)
            {
                return Result<List<PermissionReqRespModel>>.SystemError("Error retrieving permissions");
            }
        }

        public async Task<Result<PermissionReqRespModel>> UpdatePermission(PermissionReqRespModel permissionReqRespModel)
        {
            try
            {
                var permission = await _db.Permissions.FirstOrDefaultAsync(row => row.Id == permissionReqRespModel.Id);

                if (permission == null)
                {
                    return Result<PermissionReqRespModel>.ValidationError("Permission does not exist.");
                }

                permission.Name = permissionReqRespModel.Name;

                await _db.SaveChangesAsync();

                var result = new PermissionReqRespModel { Id = permission.Id, Name = permission.Name };

                return Result<PermissionReqRespModel>.Success(result);
            }
            catch (Exception ex)
            {
                return Result<PermissionReqRespModel>.SystemError("Error updating permission");
            }
        }

        public async Task<Result<PermissionReqRespModel>> DeletePermissionById(int Id) {
            try
            {
                var permission = await _db.Permissions
                    .FirstOrDefaultAsync(p => p.Id == Id);

                if (permission == null)
                {
                    return Result<PermissionReqRespModel>.NotFound("Permission not found.");
                }

                bool isUsed = await _db.RolePermissions.AnyAsync(rp => rp.RoleId == Id);
                
                if (isUsed)
                {
                    return Result<PermissionReqRespModel>.ValidationError("Permission is assigned to a role.");
                }

                _db.Permissions.Remove(permission);
                await _db.SaveChangesAsync();

                var result = new PermissionReqRespModel { Id = Id , Name = permission.Name};

                return Result<PermissionReqRespModel>.Success(result, "Permission is deleted.");
            }
            catch (Exception ex)
            {
                return Result<PermissionReqRespModel>.SystemError("Error retrieving permission.");
            }
        }
    }
}
