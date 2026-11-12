# RECONNAISSANCE — BDMS_CSharp

**Source ref:** f3448db (HEAD, Development-v1)  
**Date:** 2026-11-11  
**Profile:** standard  
**Prior runs:** none

---

## Agent 1a — Product, stack, and local operation

**Product type:** Blood Donation Management System. A backend REST API serving hospital staff (admin, staff), blood donors, and registered users.

**Trust-sensitive actions:**
- Login/logout (credential handling)
- Role/permission assignment (privilege escalation surface)
- Donation status transitions (business-state manipulation; completion triggers inventory creation)
- Blood inventory consumption (affects physical blood supply records)
- Medical record creation/update (PHI — HIV/hepatitis/malaria/syphilis results)
- User account management (create/update/delete/status)
- Dashboard data access (aggregated per-user data)

**Languages/frameworks:** C# / ASP.NET Core 8 / MediatR / EF Core 8 / SQL Server 2022  
**Build system:** .NET 8 SDK (`dotnet build`, `dotnet test`, `dotnet run`)  
**Entry point:** `BDMS.Api/Program.cs`

**Offline build/test commands:**
- `dotnet test` — xUnit + EF InMemory; no external dependencies needed
- `dotnet run --project BDMS.Api` with `DatabaseProvider=InMemory` — full API without SQL Server

**Missing local toolchains:** Docker required for SQL Server mode; EF InMemory mode is fully local.

---

## Agent 1b — Principals, authority, and controls

### Role model (source-grounded)

Roles are stored in the `Roles` table (SQL) and represented in code as plain string names:

| Role | Source definition | Authority |
|---|---|---|
| `admin` | `Roles` table row; JWT `ClaimTypes.Role = "admin"` | `PermissionFilter.cs:41` — full bypass of all permission checks |
| `staff` | `Roles` table row; JWT role claim | Subset of `Role_Permissions` rows; needs matching Permission name |
| `donor` | `Roles` table row; JWT role claim | Subset of `Role_Permissions`; can donate/book if granted |
| `user` | `Roles` table row; JWT role claim | Minimal; register/login/view own data if granted |

**There is no C# enum for roles.** Roles are compared as strings. Source: `PermissionAttribute.cs:41` (`user.IsInRole("admin")`), `AuthController.cs:61` (`new[] { "admin", "staff" }.Contains(role.ToLower())`).

### Authentication mechanism

- JWT Bearer, HMAC-SHA256, validated by `Microsoft.IdentityModel.Tokens` (`FeatureManager.cs:91–102`)
- Tokens delivered as HttpOnly cookies (`BDMS_Admin_Token` / `BDMS_Client_Token`) OR `Authorization: Bearer` header
- `OnMessageReceived` reads admin cookie first, then client cookie (`FeatureManager.cs:106–117`)
- Token generation: `TokenService.cs:21–58`
  - Claims: `NameIdentifier` = base64(userId), `Name` = username, `Email` = base64(email), `Role` = roleName, `Permission` = per-permission, `HospitalId` = base64(hospitalId) if set
  - Expiry: 60 minutes (configurable via `Jwt.ExpireMinutes`)
  - Signing key: symmetric, from `appsettings.json` `Jwt.Key`

### Authorization mechanism

Single enforcement point: `PermissionFilter.OnActionExecutionAsync` (`PermissionAttribute.cs:30–75`):

1. **Authn check:** `user.Identity.IsAuthenticated != true` → 401 (`PermissionAttribute.cs:34–38`)
2. **Admin bypass:** `user.IsInRole("admin")` → allow (`PermissionAttribute.cs:41–45`)
3. **Live DB lookup:** Decrypts `NameIdentifier` claim → parses userId → queries `Users → Role → RolePermissions → Permission.Name` → allows if match (`PermissionAttribute.cs:48–65`)
4. **Fallback:** 403

**SPEC.md §2 step 3 ("JWT has a matching Permission claim") is NOT implemented.** The `Permission` claims are added to the token (`TokenService.cs:37–40`) but `PermissionFilter` never reads them. Only the live DB lookup is performed. This means mid-session permission revocations ARE enforced (a strength), but it also means the claim-based fast path described in the spec is absent.

### Password hashing

- PBKDF2/HMAC-SHA256, 100,000 iterations, 32-byte key — `DevCode.cs:51–70`
- **Static salt:** `PasswordHasherKeys.Salt = Encoding.UTF8.GetBytes("BDMS_STATIC_SALT")` — `DevCode.cs:95` — same salt for every user
- `VerifyPassword` does extract the first 16 bytes as salt from the stored hash (`DevCode.cs:75–76`) and re-derives, then uses `CryptographicOperations.FixedTimeEquals` — but since `HashPassword` always uses the global static salt, the stored "salt" bytes and the static salt are always identical

### "Encryption" of JWT claims

- `EncryptionHelper.Encrypt` = `Convert.ToBase64String(UTF8.GetBytes(plainText))` — `DevCode.cs:107–110`
- `EncryptionHelper.Decrypt` = `Convert.FromBase64String(encrypted)` then `UTF8.GetString` — `DevCode.cs:100–104`
- **This is encoding, not encryption.** UserId, email, and hospitalId in JWT claims are trivially base64-decoded by any party who has the token.

---

## Agent 1c — Entry surfaces, copies, and sinks

### HTTP surface (all endpoints)

| Controller | Method | Path | Auth |
|---|---|---|---|
| Auth | POST | /api/auth/login | none |
| Auth | POST | /api/auth/logout | none |
| Auth | GET | /api/auth/me | [Authorize] (implicit via role check at L61) |
| UserAuth | POST | /api/userauth/login | none |
| UserAuth | POST | /api/userauth/register | none |
| UserAuth | POST | /api/userauth/logout | none |
| UserAuth | GET | /api/userauth/me | [Authorize] |
| UserAuth | POST | /api/userauth/refresh-token | [Authorize] |
| Donation | GET | /api/donation/List | donations.view |
| Donation | GET | /api/donation/{HospitalId}/{DonationDate} | donations.view |
| Donation | GET | /api/donation/Edit?DonationId={id} | donations.view |
| Donation | POST | /api/donation/Create | donations.create |
| Donation | PUT | /api/donation/Update | donations.update |
| Donation | PATCH | /api/donation/UpdateStatus | donations.approve |
| Donation | DELETE | /api/donation/Delete?DonationId={id} | donations.delete |
| Appointment | GET | /api/appointment/list | appointments.view |
| Appointment | GET | /api/appointment/{id} | appointments.view |
| Appointment | POST | /api/appointment/donation/{donationId} | appointments.create |
| Appointment | PATCH | /api/appointment/{id}/status | appointments.update |
| Appointment | PATCH | /api/appointment/{id}/time | appointments.update |
| Appointment | POST | /api/appointment/{id}/complete | appointments.complete |
| Appointment | DELETE | /api/appointment/{id} | appointments.delete |
| BloodRequest | GET | /api/bloodrequest/list | blood_requests.view |
| BloodRequest | GET | /api/bloodrequest/{id} | blood_requests.view |
| BloodRequest | POST | /api/bloodrequest/create | blood_requests.create |
| BloodRequest | PUT | /api/bloodrequest/update | blood_requests.update |
| BloodRequest | PATCH | /api/bloodrequest/{id}/status | blood_requests.approve |
| BloodRequest | DELETE | /api/bloodrequest/{id} | blood_requests.delete |
| BloodInventory | GET | /api/bloodinventory/list | blood_inventories.view |
| BloodInventory | GET | /api/bloodinventory/{id} | blood_inventories.view |
| BloodInventory | GET | /api/bloodinventory/hospital/{hospitalId} | blood_inventories.view |
| BloodInventory | GET | /api/bloodinventory/available-stock | blood_inventories.view |
| BloodInventory | POST | /api/bloodinventory/add/{donationId} | blood_inventories.create |
| BloodInventory | PATCH | /api/bloodinventory/use | blood_inventories.update |
| BloodInventory | POST | /api/bloodinventory/stock-take | blood_inventories.update |
| BloodInventory | DELETE | /api/bloodinventory/{id} | blood_inventories.delete |
| Donor | GET | /api/donor/list | donors.view |
| Donor | GET | /api/donor/{id} | donors.view |
| Donor | POST | /api/donor/create | donors.create |
| Donor | PUT | /api/donor/update | donors.update |
| Donor | PATCH | /api/donor/{id}/status | donors.update |
| Donor | DELETE | /api/donor/delete?donorId={id} | donors.delete |
| Dashboard | GET | /api/dashboard/{userId} | [Authorize] |
| Certificate | POST | /api/certificate/Generate | certificates.create |
| Certificate | GET | /api/certificate/list | certificates.view |
| Certificate | GET | /api/certificate/{id} | certificates.view |
| Certificate | GET | /api/certificate/donor/{donorId} | certificates.view |
| Announcement | GET | /api/announcement/List | announcements.view |
| Announcement | GET | /api/announcement/{id} | announcements.view |
| Announcement | POST | /api/announcement | announcements.create |
| Announcement | PUT | /api/announcement/{id} | announcements.update |
| Announcement | DELETE | /api/announcement/{id} | announcements.delete |
| Hospital | GET | /api/hospital/list | hospitals.view |
| Hospital | GET | /api/hospital/get?id={id} | hospitals.view |
| Hospital | PUT | /api/hospital/update | hospitals.update |
| Hospital | DELETE | /api/hospital/delete?id={id} | hospitals.delete |
| MedicalRecord | GET | /api/medicalrecord/list | medical_records.view |
| MedicalRecord | GET | /api/medicalrecord/{id} | medical_records.view |
| MedicalRecord | POST | /api/medicalrecord/create | medical_records.create |
| MedicalRecord | PUT | /api/medicalrecord/update | medical_records.update |
| MedicalRecord | DELETE | /api/medicalrecord/{id} | medical_records.delete |
| Permission | GET | /api/permission/list | permissions.view |
| Permission | POST | /api/permission/create | permissions.manage |
| Permission | PUT | /api/permission/edit | permissions.manage |
| Permission | DELETE | /api/permission/delete | permissions.manage |
| RolePermission | GET | /api/rolepermission/list | permissions.view |
| RolePermission | POST | /api/rolepermission/create | permissions.manage |
| RolePermission | PUT | /api/rolepermission/edit | permissions.manage |
| RolePermission | DELETE | /api/rolepermission/delete | permissions.manage |
| Role | GET | /api/role/list | roles.view |
| Role | POST | /api/role/create | roles.create |
| Role | PUT | /api/role/edit | roles.update |
| Role | DELETE | /api/role/delete | roles.delete |
| User | GET | /api/user/list | users.view |
| User | GET | /api/user/{id} | users.view |
| User | POST | /api/user/create | users.create |
| User | PUT | /api/user/update | users.update |
| User | PATCH | /api/user/{id}/status | users.update |
| User | DELETE | /api/user/delete/{userId} | users.delete |

**Total: 54 endpoints**

### Security-relevant sinks

- `AppDbContext` (EF Core parameterized) — SQL injection risk is low by default
- Cookie write: `Response.Cookies.Append(adminCookie/clientCookie, token, HttpOnly+Secure+SameSite=None)` — `AuthController.cs:37–40`, `UserAuthController.cs:38–41`
- Log file: `log/BDMS.Api.log` (Serilog) — `Program.cs:12`

---

## Agent 1d — Local execution and deployment visibility

### Offline test capability

- `dotnet test Testing/` with `DatabaseProvider=InMemory` — xUnit suite runs fully offline
- Tests construct `AppDbContext` directly with InMemory provider + Moq services
- Suitable for sandboxed validation of RBAC logic

### Prohibited operations (for this audit run)

- `docker compose up` — requires Docker daemon; contacts Docker Hub
- `dotnet run` against remote somee.com SQL Server (`appsettings.json:11`) — contacts external paid service; prohibited
- Publishing artifacts or pushing to git remote

### Deployed controls not visible in source

- Whether `Secure` cookie flag is enforced by TLS termination proxy in production
- Whether the CORS origin `http://localhost:5173` is the production frontend URL (appears to be dev-only)
- Whether `appsettings.json` values (JWT key, DB credentials) are overridden by environment variables in production deployment
- Rate limiting / WAF / network-level controls

### Sandbox assessment

The standard sandbox (read-only target, no network, scratch-only writes) supports static analysis fully. Dynamic testing via `dotnet test` with InMemory is feasible within the sandbox if the .NET 8 SDK is available locally. No target-controlled execution has been performed in this stage.
