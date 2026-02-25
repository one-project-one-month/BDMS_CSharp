
```bash
dotnet ef dbcontext scaffold "Server=.;Database=BDMS;User ID=sa;Password=sasa@123;TrustServerCertificate=True;" Microsoft.EntityFrameworkCore.SqlServer -o AppDbContextModels -c AppDbContext -f
```

### Backend
- **.NET 8.0**
- **ASP.NET Core Web API** 
- **Entity Framework Core 9.0.9** 
- **MediatR 13.1.0** - Mediator pattern implementation

# Installed Packages
MediatR - 13.1.0
EntityFrameworkCore - 9.0.9
EntityFrameworkCore.Design - 9.0.9
EntityFrameworkCore.SqlServer - 9.0.9
EntityFrameworkCore.Tools - 9.0.9
