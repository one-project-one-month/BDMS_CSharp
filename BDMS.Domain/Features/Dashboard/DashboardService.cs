using BDMS.Shared;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDMS.Domain.Features.Dashboard
{
    public class DashboardService : IDashboardService
    {
        private readonly IConfiguration _config;

        public DashboardService(IConfiguration config)
        {
            _config = config;
        }

        public async Task<Result<string>> GetDashboardById(int user_id, CancellationToken ct)
        {
            using IDbConnection db = new SqlConnection(_config.GetConnectionString("DefaultConnection"));

            var command = new CommandDefinition("sp_GetDashboardData",
                                                new { user_id = user_id },
                                                commandType: CommandType.StoredProcedure,
                                                cancellationToken : ct);

            try
            {
                var result = await db.QueryFirstOrDefaultAsync<string>(command);

                return Result<string>.Success(result ?? "{}");
            }
            catch (Exception ex) 
            {
                return Result<string>.SystemError("Failed to load dashboard", ex.Message);
            }
        }
    }
}
