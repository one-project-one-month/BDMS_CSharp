# BDMS C# API (Clean Architecture)

## Layers
- **Domain**: Entities and core business rules (`User`, `BloodDonor`, `CanDonate`).
- **Application**: Use-cases, service contracts, and CQRS handlers.
- **Infrastructure**: EF Core `AppDbContext` and SQL Server configuration.
- **API**: Controllers and dependency injection wiring.

## Tech Stack
- .NET 8 Web API
- Entity Framework Core + SQL Server
- MediatR for CQRS (donor feature only)

## Run
1. Update `BDMS.Api/appsettings.json` with your SQL Server connection string.
2. Run the API.
3. Use `BDMS.Api/BDMS.Api.http` to test endpoints.
