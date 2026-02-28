# BDMS C# Setup

## Prerequisites
- Docker Desktop (or Docker Engine + Compose)
- .NET SDK 8+

## 1) Start SQL Server 2022 in Docker
From repository root:

```bash
docker compose up -d
```

Check container status:

```bash
docker compose ps
```

If needed, view SQL Server startup logs:

```bash
docker compose logs -f mssql
```

> SQL Server is ready when logs show it is accepting client connections.

## 2) Install EF Core CLI (one time)
If `dotnet ef` is not available:

```bash
dotnet tool install --global dotnet-ef
```

If already installed, update it:

```bash
dotnet tool update --global dotnet-ef
```

## 3) Create or update BDMSDb schema
Run migration update command:

```bash
dotnet ef database update --project BDMS.Infrastructure --startup-project BDMS.Api
```

This uses the existing `DefaultConnection` in `BDMS.Api/appsettings.json`:

`Server=localhost,1433;Database=BDMSDb;User Id=sa;Password=BDMS@20262802;TrustServerCertificate=True`

## 4) When model changes (team workflow)
Create a new migration:

```bash
dotnet ef migrations add <MigrationName> --project BDMS.Infrastructure --startup-project BDMS.Api --output-dir Data/Migrations
```

Apply it to local DB:

```bash
dotnet ef database update --project BDMS.Infrastructure --startup-project BDMS.Api
```

## New developer quick start
1. Clone repository.
2. Start SQL Server:
   ```bash
   docker compose up -d
   ```
3. Install/update EF Core CLI:
   ```bash
   dotnet tool update --global dotnet-ef
   ```
4. Apply database migrations:
   ```bash
   dotnet ef database update --project BDMS.Infrastructure --startup-project BDMS.Api
   ```
5. Run API:
   ```bash
   dotnet run --project BDMS.Api
   ```
