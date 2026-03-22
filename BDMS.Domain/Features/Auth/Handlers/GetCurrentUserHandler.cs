using BDMS.Database.AppDbContextModels;
using BDMS.Domain.Features.Auth.Models;
using BDMS.Domain.Features.Auth.Queries;
using BDMS.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDMS.Domain.Features.Auth.Handlers
{
    public class GetCurrentUserHandler : IRequestHandler<GetCurrentUserQuery, Result<LoginResModel>>
    {
        private readonly AppDbContext _db;
        public GetCurrentUserHandler(AppDbContext db)
        {
            _db = db;
        }
        public async Task<Result<LoginResModel>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
        {
            var user = await _db.Users
                                    .AsNoTracking()
                                    .Include(u => u.Role)
                                        .ThenInclude(r => r.RolePermissions)
                                        .ThenInclude(rp => rp.Permission)
                                    .FirstOrDefaultAsync(u => u.Id == request.UserId && u.IsActive, cancellationToken);
            if (user == null)
                return Result<LoginResModel>.NotFound("User not found");

            var permissions = user.Role.RolePermissions.Select(rp => rp.Permission.Name).ToList();
            var donor = await _db.Donors
                                 .AsNoTracking()
                                 .FirstOrDefaultAsync(d => d.UserId == user.Id && d.IsActive && d.DeletedAt == null, cancellationToken);

            var result = new LoginResModel
            {
                UserId = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                RoleName = user.Role.Name,
                Permissions = permissions,
                Donor = donor == null
                    ? null
                    : new CurrentUserDonorResModel
                    {
                        Id = donor.Id,
                        UserId = donor.UserId,
                        NicNo = donor.NicNo,
                        DateOfBirth = donor.DateOfBirth,
                        Gender = donor.Gender,
                        BloodGroup = donor.BloodGroup,
                        LastDonationDate = donor.LastDonationDate,
                        Remarks = donor.Remarks,
                        EmergencyContact = donor.EmergencyContact,
                        EmergencyPhone = donor.EmergencyPhone,
                        Address = donor.Address,
                        IsActive = donor.IsActive,
                        CreatedAt = donor.CreatedAt,
                        UpdatedAt = donor.UpdatedAt,
                        DeletedAt = donor.DeletedAt
                    }
            };

            return Result<LoginResModel>.Success(result);
        }
    }
}
