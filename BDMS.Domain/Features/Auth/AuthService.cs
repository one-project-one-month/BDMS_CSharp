using BDMS.Database.AppDbContextModels;
using BDMS.Domain.Features.Auth.Commands;
using BDMS.Domain.Features.Auth.Models;
using BDMS.Shared;
using BDMS.Shared.Service;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDMS.Domain.Features.Auth
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _dbContext;
        private readonly TokenService _tokenService;

        private static readonly string[] AdminRoles = { "admin", "staff" };
        public AuthService(AppDbContext dbContext, TokenService tokenService)
        {
            _dbContext = dbContext;
            _tokenService = tokenService;
        }
        #region Admin
        public async Task<Result<LoginResultInternal>> Login(AdminLoginCommand request, CancellationToken cancellationToken)
        {
            var user = await _dbContext.Users
                                        .Include(u => u.Role)
                                            .ThenInclude(r => r.RolePermissions)
                                            .ThenInclude(rp => rp.Permission)
                                        .FirstOrDefaultAsync(u => u.Email == request.Email && u.IsActive, cancellationToken);

            if (user is null)
                return Result<LoginResultInternal>.ValidationError("Request cannot be null");

            if (request.Email is null)
                return Result<LoginResultInternal>.ValidationError("Email is required");

            if (request.Password is null)
                return Result<LoginResultInternal>.ValidationError("Password is required");

            if (!AdminRoles.Contains(user.Role.Name.ToLower()))
                return Result<LoginResultInternal>.ValidationError("This is an invalid admin & staff account");

            if (!user.Password.VerifyPassword(request.Password))
                return Result<LoginResultInternal>.ValidationError("Invalid password");

            var permissions = user.Role.RolePermissions.Select(rp => rp.Permission.Name).ToList();

            var (token, expiration) = _tokenService.GenerateToken(user, user.Role.Name, permissions);

            var result = new LoginResultInternal
            {
                UserInfo = new LoginResModel
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    RoleName = user.Role.Name,
                    Permissions = permissions
                },
                Token = token,
                ExpireToken = expiration
            };
            return Result<LoginResultInternal>.Success(result, "Login successful.");
        }
        #endregion
    }
}
