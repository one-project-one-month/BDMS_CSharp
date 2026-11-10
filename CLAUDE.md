# CLAUDE.md — Agent guide for BDMS_CSharp

Blood Donation Management System: ASP.NET Core 8 Web API, MediatR CQRS, EF Core + SQL Server.
Full system spec (endpoints, domain rules, flows): **[SPEC.md](SPEC.md)**.

## Commands

```bash
docker compose up -d              # SQL Server 2022 (colima/docker) on localhost,1433, sa/BDMS@2026, db BDMS
dotnet restore
dotnet run --project BDMS.Api     # http://localhost:5017 — Swagger at /swagger (no "/" route)
dotnet test                       # all tests; Testing/ is xUnit + Moq + EF InMemory
dotnet test --filter "FullyQualifiedName~SomeTest"
```

Query the DB via `docker exec bdms-sqlserver ... /opt/mssql-tools*/bin/sqlcmd -S localhost -U sa -P 'BDMS@2026' -C -d BDMS -Q "..."`.

## Git

- **`Development-v1` is the main branch** — not `main` (frozen since 2026-03-22). Base branches on `origin/Development-v1`; PRs merge into it.
- Never push without explicit permission; commit only when asked.

## Architecture

Request flow: **Controller → MediatR command/query → Handler → `I*Service` → `AppDbContext`**.

Everything is wired in `BDMS.Domain/FeatureManager.cs` (`builder.AddDomain()`): scoped services, MediatR assembly scan, JWT/cookie auth, **and all authorization policies** (not in `Program.cs`).

Feature modules live at `BDMS.Domain/Features/<Area>/`:

```
Commands/   Queries/   Handlers/   Models/   <Area>Service.cs + I<Area>Service.cs
```

To add an endpoint: model in `Models/`, command/query in `Commands|Queries/`, handler delegating to the service, logic in `Service`, then a controller action in `BDMS.Api/Controllers` with `[Authorize(Policy = ...)]` and `return Execute(result);`.

Key files:

- `BDMS.Shared/Result.cs` — `Result<T>` envelope `{isSuccess, isError, data, message}`
- `BDMS.Shared/DevCode.cs` — PBKDF2 password hashing (static salt, 100k iterations)
- `BDMS.Shared/Enums/*` — statuses + `ToDatabaseValue()`; DB stores lowercase strings
- `BDMS.Database/AppDbContextModels/` — entities; `Program.cs` runs `InMemoryDbSeeder` when `DatabaseProvider=InMemory`

## Conventions

- Controllers: `Route("api/[controller]")`, thin — authorization attribute + `_mediator.Send` + `Execute(result)`.
- Policies (note the intentional-by-now typo): `AdminOnly`, `StaffOnly`, `DonarOnly`, `ClientOnly`, `AdminDonar` (admin+donor), `AdminClient`, `AdminClientDonar`. Roles: `admin, staff, donor, user`.
- Auth tokens are HttpOnly cookies (`BDMS_Admin_Token` / `BDMS_Client_Token`); `Authorization: Bearer` header works too.
- Soft delete: `deleted_at` / `DeletedAt == null` filters in every query.
- Tests: construct the service with an InMemory `AppDbContext` + `Mock<IBloodInventoryService>` — see `Testing/AppointmentServiceTests.cs` and `Testing/DonationServiceTests.cs`.

## Gotchas (verified the hard way)

- **Config**: `appsettings.json` points at a stale remote somee.com server; the dev override (`appsettings.Development.json`) is the localhost connection string that actually works.
- **DB naming is snake_case**: columns `donation_id`, `deleted_at`, …; table `Blood_Inventories` (DbSet is `BloodInventories`). Filtering on `DeletedAt` in raw SQL fails — use `deleted_at`.
- **`BDMS_seed.sql` is broken for users 3–4**: it stores bcrypt hashes but `VerifyPassword` parses base64 PBKDF2 → donor/staff login throws "not a valid Base-64 string". Regenerate with `DevCode.HashPassword` (see `InMemoryDbSeeder` for the intended accounts/passwords).
- **Two donation-completion paths**: `PATCH /api/donation/UpdateStatus → completed` *and* `POST /api/appointment/{id}/complete` both create the inventory row. `CompleteAppointment` is idempotent (skips duplicate inventory) as of `fix/appointment-complete-idempotency` — keep it that way; don't reintroduce a hard error on existing inventory.
- **Appointments require donation `approved`** before booking, and `confirmed → completed` via PATCH is rejected by design — completion is the `/complete` endpoint only (AdminOnly).
- `PATCH /api/donation/UpdateStatus` has **no transition validation** — it sets whatever status is sent.
- `BloodInventoryController` and `DashboardController` have no `[Authorize]` (anonymous); no fallback auth policy is configured.
- Validation errors sometimes come back as `text/plain`, not JSON — parse defensively in tests/scripts.
