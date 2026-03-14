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

                var role = new RolesReqRespModel { Name = rolesReqRespModel.Name };

                await _db.AddAsync(role);
                await _db.SaveChangesAsync();

                var result = new RolesReqRespModel { Id = role.Id, Name = role.Name };

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

                await _db.SaveChangesAsync();

                var result = new RolesReqRespModel { Id = role.Id, Name = role.Name };

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
