using BDMS.Database.AppDbContextModels;
using BDMS.Domain.Features.Auth.Commands;
using BDMS.Domain.Features.Auth.Models;
using BDMS.Shared;
using BDMS.Shared.Service;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDMS.Domain.Features.Auth.Handlers
{
    public class AdminLoginHandler : IRequestHandler<AdminLoginCommand, Result<LoginResultInternal>>
    {
        private readonly IAuthService _authService;
        public AdminLoginHandler(IAuthService authService)
        {
            _authService = authService;
        }
        public async Task<Result<LoginResultInternal>> Handle(AdminLoginCommand request, CancellationToken cancellationToken)
        {
            return await _authService.Login(request,cancellationToken);
        }
    }
}
