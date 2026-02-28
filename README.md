# BDMS C# Setup

## Prerequisites
- Docker Desktop (or Docker Engine with Compose)
- .NET SDK 8+
- EF Core CLI (`dotnet tool install --global dotnet-ef`)

## Run SQL Server 2022 in Docker
From repository root:

```bash
docker compose up -d
```

This starts SQL Server 2022 on `localhost,1433` and persists data in the `mssql_data` named volume.

## Apply EF Core migrations
Use the API project as startup and Infrastructure as migrations project:

```bash
dotnet ef database update --project BDMS.Infrastructure --startup-project BDMS.Api
```

This creates or updates the `BDMSDb` database and tables using the configured `DefaultConnection`.

## New developer quick start
1. Clone repository.
2. Start SQL Server:
   ```bash
   docker compose up -d
   ```
3. Apply database migrations:
   ```bash
   dotnet ef database update --project BDMS.Infrastructure --startup-project BDMS.Api
   ```
4. Run API:
   ```bash
   dotnet run --project BDMS.Api
   ```
