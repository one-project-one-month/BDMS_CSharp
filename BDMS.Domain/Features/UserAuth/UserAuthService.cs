using BDMS.Database.AppDbContextModels;
using BDMS.Domain.Features.UserAuth.Commands;
using BDMS.Domain.Features.UserAuth.Models;
using BDMS.Shared;
using BDMS.Shared.Service;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDMS.Domain.Features.UserAuth
{
    public class UserAuthService : IUserAuthService
    {
        private readonly AppDbContext _dbContext;
        private readonly TokenService _tokenService;

        private const string UserRoleName = "user";

        public UserAuthService(AppDbContext dbContext, TokenService tokenService)
        {
            _dbContext = dbContext;
            _tokenService = tokenService;
        }

        public async Task<Result<UserLoginResultInternal>> Login(UserLoginCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _dbContext.Users
                                            .Include(u => u.Role)
                                                .ThenInclude(r => r.RolePermissions)
                                                .ThenInclude(rp => rp.Permission)
                                            .FirstOrDefaultAsync(u => u.Email == request.Email && u.IsActive, cancellationToken);

                if (user is null)
                {
                    return Result<UserLoginResultInternal>.ValidationError("Invalid email or password");
                }

                if (!user.Password.VerifyPassword(request.Password))
                {
                    return Result<UserLoginResultInternal>.ValidationError("Invalid password");
                }

                var permissions = user.Role.RolePermissions.Select(rp => rp.Permission.Name).ToList();

                var (token, expiration) = _tokenService.GenerateToken(user, user.Role.Name, permissions);

                var result = new UserLoginResultInternal
                {
                    UserInfo = new UserAuthResModel
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
                return Result<UserLoginResultInternal>.Success(result, "Login successful.");
            }
            catch (Exception ex)
            {
                return Result<UserLoginResultInternal>.SystemError($"An error occurred during login: {ex.Message}");
            }
        }

        public async Task<Result<UserLoginResultInternal>> Register(UserRegisterCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (request.Password != request.ConfirmPassword)
                {
                    return Result<UserLoginResultInternal>.ValidationError("Passwords do not match.");
                }

                var existingUser = await _dbContext.Users.AnyAsync(u => u.Email == request.Email, cancellationToken);
                if (existingUser)
                {
                    return Result<UserLoginResultInternal>.ValidationError("Email already exists.");
                }

                var role = await _dbContext.Roles.FirstOrDefaultAsync(r => r.Name.ToLower() == UserRoleName, cancellationToken);
                if (role == null)
                {
                    return Result<UserLoginResultInternal>.ValidationError("Default user role not found.");
                }

                var newUser = new BDMS.Database.AppDbContextModels.User
                {
                    UserName = request.UserName,
                    Email = request.Email,
                    Password = request.Password.HashPassword(),
                    RoleId = role.Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _dbContext.Users.Add(newUser);
                await _dbContext.SaveChangesAsync(cancellationToken);

                var userWithRole = await _dbContext.Users
                                                    .Include(u => u.Role)
                                                        .ThenInclude(r => r.RolePermissions)
                                                        .ThenInclude(rp => rp.Permission)
                                                    .FirstAsync(u => u.Id == newUser.Id, cancellationToken);

                var userPermissions = userWithRole.Role.RolePermissions.Select(rp => rp.Permission.Name).ToList();
                var (token, expiration) = _tokenService.GenerateToken(userWithRole, userWithRole.Role.Name, userPermissions);

                var result = new UserLoginResultInternal
                {
                    UserInfo = new UserAuthResModel
                    {
                        UserId = userWithRole.Id,
                        UserName = userWithRole.UserName,
                        Email = userWithRole.Email,
                        RoleName = userWithRole.Role.Name,
                        Permissions = userPermissions
                    },
                    Token = token,
                    ExpireToken = expiration
                };

                return Result<UserLoginResultInternal>.Success(result, "Registration successful.");
            }
            catch (Exception ex)
            {
                return Result<UserLoginResultInternal>.SystemError($"An error occurred during registration: {ex.Message}");
            }
        }
    }
}
