# Complete Endpoint Inventory — BDMS_CSharp

**Source ref:** f3448db (HEAD, Development-v1)  
**Total endpoints:** 79  
**Route source:** Controller code (primary); reconciled against SPEC.md §3 endpoint map

---

> [!NOTE]
> Authn requirement legend:
> - **none** = no `[Authorize]` or `[Permission]` on method; anonymous access allowed
> - **[Authorize]** = JWT required (any valid authenticated user); no permission check
> - **[Permission("x.y")]** = `PermissionFilter` applied — requires auth AND (admin role OR live `Role_Permissions` row for `x.y`)

---

## AUTH controller — `api/auth`

**Handler:** `BDMS.Api/Controllers/AuthController.cs`  
**Purpose:** Admin/staff login; sets `BDMS_Admin_Token` cookie

| # | Method | Full URL | Handler method | Authn | Authz / Permission | Middleware | Ownership check | Request validation | Sensitive response fields | Expected status |
|---|---|---|---|---|---|---|---|---|---|---|
| 1 | POST | /api/auth/login | `AuthController.Login` | **none** | none | CORS, JWT middleware (no-op if no token) | n/a | `AdminLoginReqModel` (email, password) — no length/format constraints visible in controller | JWT token (returned in body), JWT also set as HttpOnly cookie | 200 (success), 401 (bad credentials) |
| 2 | POST | /api/auth/logout | `AuthController.Logout` | **none** | none | CORS | n/a | none | none | 200 |
| 3 | GET | /api/auth/me | `AuthController.Me` | **[Authorize] (implicit)** — `ClaimTypes.Role` checked at L61 | admin or staff role in JWT | CORS, JWT auth | Caller's own userId extracted from token | none | User profile data | 200, 401 |

> [!WARNING]
> `GET /api/auth/me` has no `[Authorize]` attribute. It performs a manual role check on `User.FindFirstValue(ClaimTypes.Role)` (L59–64). If `User.Identity.IsAuthenticated` is false, `ClaimTypes.Role` will be null/empty, and the endpoint returns 401 — but this is a **manual check, not enforced by ASP.NET Core middleware**. Source: `AuthController.cs:59–64`.

---

## USERAUTH controller — `api/userauth`

**Handler:** `BDMS.Api/Controllers/UserAuthController.cs`  
**Purpose:** Donor/user login, registration, token refresh; sets `BDMS_Client_Token` cookie

| # | Method | Full URL | Handler method | Authn | Authz | Ownership check | Request validation | Sensitive response fields | Expected status |
|---|---|---|---|---|---|---|---|---|---|
| 4 | POST | /api/userauth/login | `UserAuthController.Login` | **none** | none | n/a | `UserLoginReqModel` | JWT token | 200, 401 |
| 5 | POST | /api/userauth/register | `UserAuthController.Register` | **none** | none | n/a | `UserRegisterReqModel` (username, email, password, confirmPassword) | JWT token (issued immediately on register) | 200, 400 |
| 6 | POST | /api/userauth/logout | `UserAuthController.Logout` | **none** | none | n/a | none | none | 200 |
| 7 | GET | /api/userauth/me | `UserAuthController.Me` | **[Authorize]** | any authenticated user | Reads own userId from token | none | User profile | 200, 401 |
| 8 | POST | /api/userauth/refresh-token | `UserAuthController.RefreshToken` | **[Authorize]** | any authenticated user | Reads own userId from token | none | New JWT token | 200, 401 |

---

## DONATION controller — `api/donation`

**Handler:** `BDMS.Api/Controllers/DonationController.cs`  
**Note:** Controller extends `ControllerBase` (not `BaseController`). Uses manual `Ok(result)` / `BadRequest(result.Message)`.

| # | Method | Full URL | Handler method | Authn | Authz / Permission | Ownership check | Request validation | Sensitive fields | Expected status |
|---|---|---|---|---|---|---|---|---|---|
| 9 | GET | /api/donation/List | `GetAllDonation` | PermissionFilter | donations.view | **none** — returns all donations | none | Donor PII in response | 200, 400 |
| 10 | GET | /api/donation/{HospitalId}/{DonationDate} | `GetDonationByDateAndHospital` | PermissionFilter | donations.view | none | HospitalId (int route), DonationDate (DateOnly route) | Donor PII | 200, 400 |
| 11 | GET | /api/donation/Edit?DonationId={id} | `GetDonationById` | PermissionFilter | donations.view | none | DonationId (int query param) | Donor PII | 200, 400 |
| 12 | POST | /api/donation/Create | `CreateDonation` | PermissionFilter | donations.create | none — CreatedBy is a request field, not extracted from token | `DonationCreateReqModel` | none | 200, 400 |
| 13 | PUT | /api/donation/Update | `UpdateDonation` | PermissionFilter | donations.update | none | `DonationUpdateReqModel` including Status field | none | 200, 400 |
| 14 | PATCH | /api/donation/UpdateStatus | `UpdateStatus` | PermissionFilter | donations.approve | none | `UpdateDonationStatusReqModel` (id, status); enum validated (`DonationController.cs:131`) | none | 200, 400 |
| 15 | DELETE | /api/donation/Delete?DonationId={id} | `DeleteDonation` | PermissionFilter | donations.delete | none | DonationId (int query param) | none | 200, 400 |

> [!IMPORTANT]
> `PATCH /api/donation/UpdateStatus` has **no server-side transition validation** — any status value (pending, cancelled, approved, rejected, completed, screening) can be set by any caller with `donations.approve`. SPEC.md §5 explicitly notes this known gap. Source: `CLAUDE.md:59`, `SPEC.md:113`.

---

## APPOINTMENT controller — `api/appointment`

**Handler:** `BDMS.Api/Controllers/AppointmentController.cs`

| # | Method | Full URL | Handler method | Authn | Authz / Permission | Ownership check | Request validation | Sensitive fields | Expected status |
|---|---|---|---|---|---|---|---|---|---|
| 16 | GET | /api/appointment/list?hospitalId={?}&appointmentDate={?} | `GetAllAppointmentList` | PermissionFilter | appointments.view | none | hospitalId (optional int query), appointmentDate (optional date string; parsed with `DateOnly.TryParse`) | none | 200, 400 |
| 17 | GET | /api/appointment/{id} | `GetAppointmentById` | PermissionFilter | appointments.view | none | id (int route) | none | 200, 400 |
| 18 | POST | /api/appointment/donation/{donationId} | `CreateDonationAppointment` | PermissionFilter | appointments.create | none — no check that caller owns the donation | donationId (int route), `AppointmentReqModel` (remarks) | none | 200, 400 |
| 19 | PATCH | /api/appointment/{id}/status | `UpdateAppointmentStatus` | PermissionFilter | appointments.update | none | id (route), `UpdateAppointmentStatusReqModel`; enum validated | none | 200, 400 |
| 20 | PATCH | /api/appointment/{id}/time | `UpdateAppointmentTime` | PermissionFilter | appointments.update | none | id (route), date/time strings with `TryParse` | none | 200, 400 |
| 21 | POST | /api/appointment/{id}/complete | `CompleteAppointment` | PermissionFilter | appointments.complete | none | id (int route) | none | 200, 400 |
| 22 | DELETE | /api/appointment/{id} | `DeleteAppointment` | PermissionFilter | appointments.delete | none | id (int route) | none | 200, 400 |

---

## BLOODREQUEST controller — `api/bloodrequest`

**Handler:** `BDMS.Api/Controllers/BloodRequestController.cs`

| # | Method | Full URL | Handler method | Authn | Authz / Permission | Ownership check | Sensitive fields | Expected status |
|---|---|---|---|---|---|---|---|---|
| 23 | GET | /api/bloodrequest/list | `GetAllBloodRequestList` | PermissionFilter | blood_requests.view | none | patient name, contact phone | 200, 400 |
| 24 | GET | /api/bloodrequest/{id} | `GetBloodRequestById` | PermissionFilter | blood_requests.view | none | patient name, contact phone | 200, 400 |
| 25 | POST | /api/bloodrequest/create | `CreateBloodRequest` | PermissionFilter | blood_requests.create | none — UserId is a request body field | patient name, contact phone, reason | 200, 400 |
| 26 | PUT | /api/bloodrequest/update | `UpdateBloodRequest` | PermissionFilter | blood_requests.update | none | patient name, contact phone | 200, 400 |
| 27 | PATCH | /api/bloodrequest/{id}/status | `UpdateStatus` | PermissionFilter | blood_requests.approve | ApprovedByUserId extracted from token | urgency, status | 200, 400, 401 |
| 28 | DELETE | /api/bloodrequest/{id} | `DeleteBloodRequest` | PermissionFilter | blood_requests.delete | none | none | 200, 400 |

---

## BLOODINVENTORY controller — `api/bloodinventory`

**Handler:** `BDMS.Api/Controllers/BloodInventoryController.cs`

| # | Method | Full URL | Handler method | Authn | Authz / Permission | Ownership check | Expected status |
|---|---|---|---|---|---|---|---|
| 29 | GET | /api/bloodinventory/list | `GetAll` | PermissionFilter | blood_inventories.view | none | 200, 400 |
| 30 | GET | /api/bloodinventory/{id} | `GetById` | PermissionFilter | blood_inventories.view | none | 200, 400 |
| 31 | GET | /api/bloodinventory/hospital/{hospitalId} | `GetByHospital` | PermissionFilter | blood_inventories.view | none | 200, 400 |
| 32 | GET | /api/bloodinventory/available-stock?hospitalId={?}&bloodGroup={?} | `GetAvailableStock` | PermissionFilter | blood_inventories.view | none | 200, 400 |
| 33 | POST | /api/bloodinventory/add/{donationId} | `AddToInventory` | PermissionFilter | blood_inventories.create | none — no check that caller's hospital matches donation's hospital | 200, 400 |
| 34 | PATCH | /api/bloodinventory/use | `UseFromInventory` | PermissionFilter | blood_inventories.update | none | 200, 400 |
| 35 | POST | /api/bloodinventory/stock-take?hospitalId={?} | `RunStockTake` | PermissionFilter | blood_inventories.update | none | 200, 400 |
| 36 | DELETE | /api/bloodinventory/{id} | `Delete` | PermissionFilter | blood_inventories.delete | none | 200, 400 |

---

## DONOR controller — `api/donor`

**Handler:** `BDMS.Api/Controllers/DonorController.cs`

| # | Method | Full URL | Handler method | Authn | Authz / Permission | Notes | Expected status |
|---|---|---|---|---|---|---|---|
| 37 | GET | /api/donor/list | `GetAllDonorsList` | PermissionFilter | donors.view | Returns all donors with PII | 200, 400 |
| 38 | GET | /api/donor/{id} | `GetDonorById` | PermissionFilter | donors.view | **Route param `{id}` does not match method param `donorId`** (no `[FromRoute]`) — ASP.NET Core cannot bind; donorId likely always 0. See open question #5. | 200, 400 |
| 39 | POST | /api/donor/create | `CreateDonor` | PermissionFilter | donors.create | NIC No, DOB, blood group, emergency contact | 200, 400 |
| 40 | PUT | /api/donor/update | `UpdateDonor` | PermissionFilter | donors.update | No ownership check | 200, 400 |
| 41 | PATCH | /api/donor/{id}/status | `UpdateDonorStatus` | PermissionFilter | donors.update | id via `[FromRoute]`-less param (no `[FromRoute]`) — may not bind from route | 200, 400 |
| 42 | DELETE | /api/donor/delete?donorId={id} | `DeleteDonorAsync` | PermissionFilter | donors.delete | No `[FromQuery]` — relies on query string binding default | 200, 400 |

> [!CAUTION]
> `GET /api/donor/{id}` and `PATCH /api/donor/{id}/status`: method parameters are named `donorId`/`id` respectively but neither uses `[FromRoute]`. For `GetDonorById`, route param `{id}` ≠ method param `donorId` — ASP.NET Core model binder defaults to `donorId=0` (default int). **This endpoint likely always looks up donor ID 0, not the route value.** Needs validation via dynamic test.

---

## DASHBOARD controller — `api/dashboard`

**Handler:** `BDMS.Api/Controllers/DashboardController.cs`

| # | Method | Full URL | Handler method | Authn | Authz | Ownership check | Sensitive fields | Expected status |
|---|---|---|---|---|---|---|---|---|
| 43 | GET | /api/dashboard/{userId} | `GetDashboardById` | **[Authorize]** | any authenticated user — no permission required | **NONE** — `userId` from URL is passed directly to query; no check that caller's token userId matches route userId | Aggregated stats for the user | 200, 400 |

> [!CAUTION]
> Any authenticated user (even `user` or `donor` role) can supply any integer as `{userId}` and retrieve another user's dashboard data. No ownership check in controller. Source: `DashboardController.cs:19–31`. Service-layer check not yet verified (Stage 2).

---

## CERTIFICATE controller — `api/certificate`

**Handler:** `BDMS.Api/Controllers/CertificateController.cs`

| # | Method | Full URL | Handler method | Authn | Authz / Permission | Ownership check | Expected status |
|---|---|---|---|---|---|---|---|
| 44 | POST | /api/certificate/Generate | `GenerateCertificate` | PermissionFilter | certificates.create | none | 200, 400 |
| 45 | GET | /api/certificate/list | `GetAllCertificates` | PermissionFilter | certificates.view | none | 200, 400 |
| 46 | GET | /api/certificate/{id} | `GetCertificateById` | PermissionFilter | certificates.view | none | 200, 400 |
| 47 | GET | /api/certificate/donor/{donorId} | `GetCertificatesByDonorId` | PermissionFilter | certificates.view | none | 200, 400 |

---

## ANNOUNCEMENT controller — `api/announcement`

**Handler:** `BDMS.Api/Controllers/AnnouncementController.cs`

| # | Method | Full URL | Handler method | Authn | Authz / Permission | Expected status |
|---|---|---|---|---|---|---|
| 48 | GET | /api/announcement/List?category={} | `GetAnnouncements` | PermissionFilter | announcements.view | 200, 400 |
| 49 | GET | /api/announcement/{id} | `GetAnnouncementById` | PermissionFilter | announcements.view | 200, 400 |
| 50 | POST | /api/announcement | `CreateAnnouncement` | PermissionFilter | announcements.create | 200, 400 |
| 51 | PUT | /api/announcement/{id} | `UpdateAnnouncement` | PermissionFilter | announcements.update | 200, 400 |
| 52 | DELETE | /api/announcement/{id} | `DeleteAnnouncement` | PermissionFilter | announcements.delete | 200, 400 |

---

## HOSPITAL controller — `api/hospital`

**Handler:** `BDMS.Api/Controllers/HospitalController.cs`

| # | Method | Full URL | Handler method | Authn | Authz / Permission | Expected status |
|---|---|---|---|---|---|---|
| 53 | GET | /api/hospital/list | `GetAllHospitals` | PermissionFilter | hospitals.view | 200, 400 |
| 54 | GET | /api/hospital/get?id={id} | `GetHospitalById` | PermissionFilter | hospitals.view | 200, 400 |
| 55 | PUT | /api/hospital/update | `UpdateHospital` | PermissionFilter | hospitals.update | 200, 400 |
| 56 | DELETE | /api/hospital/delete?id={id} | `DeleteHospital` | PermissionFilter | hospitals.delete | 200, 400 |

---

## MEDICALRECORD controller — `api/medicalrecord`

**Handler:** `BDMS.Api/Controllers/MedicalRecordController.cs`  
**PHI:** Contains HIV, hepatitis B/C, malaria, syphilis test results

| # | Method | Full URL | Handler method | Authn | Authz / Permission | Ownership check | Expected status |
|---|---|---|---|---|---|---|---|
| 57 | GET | /api/medicalrecord/list | `GetAll` | PermissionFilter | medical_records.view | none — returns all records | 200, 400 |
| 58 | GET | /api/medicalrecord/{id} | `GetById` | PermissionFilter | medical_records.view | none | 200, 400 |
| 59 | POST | /api/medicalrecord/create | `Create` | PermissionFilter | medical_records.create | none | 200, 400 |
| 60 | PUT | /api/medicalrecord/update | `Update` | PermissionFilter | medical_records.update | none | 200, 400 |
| 61 | DELETE | /api/medicalrecord/{id} | `Delete` | PermissionFilter | medical_records.delete | none | 200, 400 |

---

## PERMISSION controller — `api/permission`

**Handler:** `BDMS.Api/Controllers/PermissionController.cs`

| # | Method | Full URL | Handler method | Authn | Authz / Permission | Expected status |
|---|---|---|---|---|---|---|
| 62 | GET | /api/permission/list | `GetAllPermissions` | PermissionFilter | permissions.view | 200, 400 |
| 63 | POST | /api/permission/create | `CreatePermission` | PermissionFilter | permissions.manage | 200, 400 |
| 64 | PUT | /api/permission/edit | `UpdatePermission` | PermissionFilter | permissions.manage | 200, 400 |
| 65 | DELETE | /api/permission/delete | `DeletePermission` | PermissionFilter | permissions.manage | 200, 400 |

---

## ROLEPERMISSION controller — `api/rolepermission`

**Handler:** `BDMS.Api/Controllers/RolePermissionController.cs`

| # | Method | Full URL | Handler method | Authn | Authz / Permission | Expected status |
|---|---|---|---|---|---|---|
| 66 | GET | /api/rolepermission/list | `GetAllRolePermissions` | PermissionFilter | permissions.view | 200, 400 |
| 67 | POST | /api/rolepermission/create | `CreateRolePermission` | PermissionFilter | permissions.manage | 200, 400 |
| 68 | PUT | /api/rolepermission/edit | `UpdateRolePermission` | PermissionFilter | permissions.manage | 200, 400 |
| 69 | DELETE | /api/rolepermission/delete | `DeleteRolePermission` | PermissionFilter | permissions.manage | 200, 400 |

---

## ROLE controller — `api/role`

**Handler:** `BDMS.Api/Controllers/RoleController.cs`

| # | Method | Full URL | Handler method | Authn | Authz / Permission | Expected status |
|---|---|---|---|---|---|---|
| 70 | GET | /api/role/list | `GetAllRoles` | PermissionFilter | roles.view | 200, 400 |
| 71 | POST | /api/role/create | `CreateRole` | PermissionFilter | roles.create | 200, 400 |
| 72 | PUT | /api/role/edit | `UpdateRole` | PermissionFilter | roles.update | 200, 400 |
| 73 | DELETE | /api/role/delete | `DeleteRole` | PermissionFilter | roles.delete | 200, 400 |

---

## USER controller — `api/user`

**Handler:** `BDMS.Api/Controllers/UserController.cs`

| # | Method | Full URL | Handler method | Authn | Authz / Permission | Ownership check | Expected status |
|---|---|---|---|---|---|---|---|
| 74 | GET | /api/user/list | `GetAllUserList` | PermissionFilter | users.view | none — returns all users with emails | 200, 400 |
| 75 | GET | /api/user/{id} | `GetUserById` | PermissionFilter | users.view | none | 200, 400 |
| 76 | POST | /api/user/create | `CreateUser` | PermissionFilter | users.create | none | 200, 400 |
| 77 | PUT | /api/user/update | `UpdateUser` | PermissionFilter | users.update | none — UserId is a request body field | 200, 400 |
| 78 | PATCH | /api/user/{id}/status | `UpdateUserStatus` | PermissionFilter | users.update | none | 200, 400 |
| 79 | DELETE | /api/user/delete/{userId} | `DeleteUser` | PermissionFilter | users.delete | none | 200, 400 |

---

## Route-source reconciliation

| Source | Endpoints found | Discrepancies |
|---|---|---|
| Controller code (primary) | 79 endpoints | Ground truth |
| SPEC.md §3 endpoint map | ~50 entries (summarized) | SPEC does not list `GET /api/hospital/get` and `DELETE /api/hospital/delete` as separate entries from "GET list / GET get"; minor naming differences |
| OpenAPI/Swagger | Dynamic (runtime-generated); no static file in repo | Cannot reconcile statically |
| Test files (`Testing/*.cs`) | Not yet read | Stage 2 open item |

**Mismatches (SPEC vs code):**
1. SPEC says `GET {id}` for Donor; code uses `GET {id}` but method param is named `donorId` — routing mismatch (open question #5)
2. SPEC says `DELETE delete` for Donor; code uses `DELETE /api/donor/delete?donorId={}` — this is a query param, not a path segment named `delete` as the SPEC implies
3. SPEC mentions `GET me` for Auth — implemented as manual auth check without `[Authorize]` attribute
4. `PermissionController.DELETE /api/permission/delete` accepts the entity as request body (no `[FromBody]` attribute in controller, relies on model binder default) — unusual pattern for DELETE

---

## Summary

- **Total endpoints:** 79 across 16 controllers
- **Anonymous routes:** 5 endpoints (admin login/logout and client login/register/logout); `Auth/me` performs a manual role check without `[Authorize]`
- **[Authorize] only:** 3 endpoints (UserAuth/me, UserAuth/refresh-token, Dashboard/{userId})
- **[Permission(x.y)] protected:** 70 endpoints
- **Unresolved traces:** 10 open items documented in AUDIT-STATE.md
- **Coverage claim:** This inventory contains all 79 controller-layer routes. Stage 3 traced selected service paths; remaining handler/service paths and dynamic validation are not claimed as fully covered.
