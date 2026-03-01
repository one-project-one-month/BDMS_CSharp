# BDMS C#

## Run Docker (SQL Server 2022)

```bash
docker compose up -d
```

SQL Server is available at `localhost,1433` with:

- User: `sa`
- Password: `BDMS@2026`
- Database: `BDMSDb`

## Start the project (new developer setup)

1. Start SQL Server:

   ```bash
   docker compose up -d
   ```

2. Restore dependencies:

   ```bash
   dotnet restore
   ```

3. Run the API:

   ```bash
   dotnet run --project BDMS.Api
   ```

4. Run tests:

   ```bash
   dotnet test
   ```
