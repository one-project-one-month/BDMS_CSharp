# Feature-Endpoint Groups — BDMS_CSharp

**Source ref:** f3448db (HEAD, Development-v1)  
**Method:** Grouped only by implemented controller/domain features and the routes in the complete endpoint inventory. No feature is inferred beyond an implemented route.
**Total endpoints:** 79 (all controller actions in the complete inventory)

---

## GROUP 1 — Admin Authentication (`api/auth`)

Covers login, session management, and current-user resolution for the **admin/staff** portal. Issues `BDMS_Admin_Token`.

| # | Method | Path | Permission |
|---|---|---|---|
| 1 | POST | /api/auth/login | none — anonymous |
| 2 | POST | /api/auth/logout | none — anonymous |
| 3 | GET | /api/auth/me | manual role check (admin/staff only) |

**Key files:**
- [`AuthController.cs`](file:///d:/onePj_oneMonth/Blood/BDMS_CSharp/BDMS.Api/Controllers/AuthController.cs)
- [`AuthService.cs`](file:///d:/onePj_oneMonth/Blood/BDMS_CSharp/BDMS.Domain/Features/Auth/AuthService.cs)
- [`GetCurrentUserHandler.cs`](file:///d:/onePj_oneMonth/Blood/BDMS_CSharp/BDMS.Domain/Features/Auth/Handlers/GetCurrentUserHandler.cs)

---

## GROUP 2 — User/Donor Authentication (`api/userauth`)

Covers login, registration, session management, and token refresh for **donor/user** portal. Issues `BDMS_Client_Token`. Self-registration assigns the `user` role automatically.

| # | Method | Path | Permission |
|---|---|---|---|
| 4 | POST | /api/userauth/login | none — anonymous |
| 5 | POST | /api/userauth/register | none — anonymous |
| 6 | POST | /api/userauth/logout | none — anonymous |
| 7 | GET | /api/userauth/me | [Authorize] |
| 8 | POST | /api/userauth/refresh-token | [Authorize] |

**Key files:**
- [`UserAuthController.cs`](file:///d:/onePj_oneMonth/Blood/BDMS_CSharp/BDMS.Api/Controllers/UserAuthController.cs)
- [`UserAuthService.cs`](file:///d:/onePj_oneMonth/Blood/BDMS_CSharp/BDMS.Domain/Features/UserAuth/UserAuthService.cs)

---

## GROUP 3 — Donation Management (`api/donation`)

Full CRUD and status transitions for blood donation records. Status transition (`PATCH UpdateStatus`) triggers inventory creation when status becomes `completed`. No server-side state machine.

| # | Method | Path | Permission |
|---|---|---|---|
| 9 | GET | /api/donation/List | donations.view |
| 10 | GET | /api/donation/{HospitalId}/{DonationDate} | donations.view |
| 11 | GET | /api/donation/Edit?DonationId={id} | donations.view |
| 12 | POST | /api/donation/Create | donations.create |
| 13 | PUT | /api/donation/Update | donations.update |
| 14 | PATCH | /api/donation/UpdateStatus | donations.approve |
| 15 | DELETE | /api/donation/Delete?DonationId={id} | donations.delete |

**Key files:**
- [`DonationController.cs`](file:///d:/onePj_oneMonth/Blood/BDMS_CSharp/BDMS.Api/Controllers/DonationController.cs)
- [`DonationService.cs`](file:///d:/onePj_oneMonth/Blood/BDMS_CSharp/BDMS.Domain/Features/Donations/DonationService.cs)

---

## GROUP 4 — Appointment Management (`api/appointment`)

Create, update, and complete donation and blood-request appointments. `POST {id}/complete` triggers inventory creation or consumption depending on appointment type.

| # | Method | Path | Permission |
|---|---|---|---|
| 16 | GET | /api/appointment/list | appointments.view |
| 17 | GET | /api/appointment/{id} | appointments.view |
| 18 | POST | /api/appointment/donation/{donationId} | appointments.create |
| 19 | PATCH | /api/appointment/{id}/status | appointments.update |
| 20 | PATCH | /api/appointment/{id}/time | appointments.update |
| 21 | POST | /api/appointment/{id}/complete | appointments.complete |
| 22 | DELETE | /api/appointment/{id} | appointments.delete |

**Key files:**
- [`AppointmentController.cs`](file:///d:/onePj_oneMonth/Blood/BDMS_CSharp/BDMS.Api/Controllers/AppointmentController.cs)
- [`AppointmentService.cs`](file:///d:/onePj_oneMonth/Blood/BDMS_CSharp/BDMS.Domain/Features/Appointment/AppointmentService.cs)

---

## GROUP 5 — Blood Request Management (`api/bloodrequest`)

CRUD and status transitions for hospital blood requests. Status change to `fulfilled` via appointment completion.

| # | Method | Path | Permission |
|---|---|---|---|
| 23 | GET | /api/bloodrequest/list | blood_requests.view |
| 24 | GET | /api/bloodrequest/{id} | blood_requests.view |
| 25 | POST | /api/bloodrequest/create | blood_requests.create |
| 26 | PUT | /api/bloodrequest/update | blood_requests.update |
| 27 | PATCH | /api/bloodrequest/{id}/status | blood_requests.approve |
| 28 | DELETE | /api/bloodrequest/{id} | blood_requests.delete |

**Key files:**
- [`BloodRequestController.cs`](file:///d:/onePj_oneMonth/Blood/BDMS_CSharp/BDMS.Api/Controllers/BloodRequestController.cs)
- [`BloodRequestService.cs`](file:///d:/onePj_oneMonth/Blood/BDMS_CSharp/BDMS.Domain/Features/BloodRequest/BloodRequestService.cs)

---

## GROUP 6 — Blood Inventory Management (`api/bloodinventory`)

Tracks available blood units per hospital. Created on donation completion, consumed on blood-request appointment completion.

| # | Method | Path | Permission |
|---|---|---|---|
| 29 | GET | /api/bloodinventory/list | blood_inventories.view |
| 30 | GET | /api/bloodinventory/{id} | blood_inventories.view |
| 31 | GET | /api/bloodinventory/hospital/{hospitalId} | blood_inventories.view |
| 32 | GET | /api/bloodinventory/available-stock | blood_inventories.view |
| 33 | POST | /api/bloodinventory/add/{donationId} | blood_inventories.create |
| 34 | PATCH | /api/bloodinventory/use | blood_inventories.update |
| 35 | POST | /api/bloodinventory/stock-take | blood_inventories.update |
| 36 | DELETE | /api/bloodinventory/{id} | blood_inventories.delete |

**Key files:**
- [`BloodInventoryController.cs`](file:///d:/onePj_oneMonth/Blood/BDMS_CSharp/BDMS.Api/Controllers/BloodInventoryController.cs)
- [`BloodInventoryService.cs`](file:///d:/onePj_oneMonth/Blood/BDMS_CSharp/BDMS.Domain/Features/BloodInventory/BloodInventoryService.cs)

---

## GROUP 7 — Donor Profile Management (`api/donor`)

CRUD for donor records. Each donor links to a `User` account via `UserId`. Creating a donor also promotes the user's role to `donor`.

| # | Method | Path | Permission |
|---|---|---|---|
| 37 | GET | /api/donor/list | donors.view |
| 38 | GET | /api/donor/{id} | donors.view |
| 39 | POST | /api/donor/create | donors.create |
| 40 | PUT | /api/donor/update | donors.update |
| 41 | PATCH | /api/donor/{id}/status | donors.update |
| 42 | DELETE | /api/donor/delete | donors.delete |

**Key files:**
- [`DonorController.cs`](file:///d:/onePj_oneMonth/Blood/BDMS_CSharp/BDMS.Api/Controllers/DonorController.cs)
- [`DonorService.cs`](file:///d:/onePj_oneMonth/Blood/BDMS_CSharp/BDMS.Domain/Features/Donor/DonorService.cs)

---

## GROUP 8 — Dashboard (`api/dashboard`)

Returns aggregated system statistics. Uses stored procedure `sp_GetDashboardData` in MSSQL mode; EF InMemory aggregate queries in InMemory mode. No role-scoped filtering.

| # | Method | Path | Permission |
|---|---|---|---|
| 43 | GET | /api/dashboard/{userId} | [Authorize] only |

**Key files:**
- [`DashboardController.cs`](file:///d:/onePj_oneMonth/Blood/BDMS_CSharp/BDMS.Api/Controllers/DashboardController.cs)
- [`DashboardService.cs`](file:///d:/onePj_oneMonth/Blood/BDMS_CSharp/BDMS.Domain/Features/Dashboard/DashboardService.cs)

---

## GROUP 9 — Donation Certificates (`api/certificate`)

Generates and retrieves PDF/data certificates for completed donors. Requires donor to have at least one donation record.

| # | Method | Path | Permission |
|---|---|---|---|
| 44 | POST | /api/certificate/Generate | certificates.create |
| 45 | GET | /api/certificate/list | certificates.view |
| 46 | GET | /api/certificate/{id} | certificates.view |
| 47 | GET | /api/certificate/donor/{donorId} | certificates.view |

**Key files:**
- [`CertificateController.cs`](file:///d:/onePj_oneMonth/Blood/BDMS_CSharp/BDMS.Api/Controllers/CertificateController.cs)
- [`CertificateService.cs`](file:///d:/onePj_oneMonth/Blood/BDMS_CSharp/BDMS.Domain/Features/Certificate/CertificateService.cs)

---

## GROUP 10 — Announcements (`api/announcement`)

CRUD for system announcements. Category-filtered reads.

| # | Method | Path | Permission |
|---|---|---|---|
| 48 | GET | /api/announcement/List | announcements.view |
| 49 | GET | /api/announcement/{id} | announcements.view |
| 50 | POST | /api/announcement | announcements.create |
| 51 | PUT | /api/announcement/{id} | announcements.update |
| 52 | DELETE | /api/announcement/{id} | announcements.delete |

**Key files:**
- [`AnnouncementController.cs`](file:///d:/onePj_oneMonth/Blood/BDMS_CSharp/BDMS.Api/Controllers/AnnouncementController.cs)
- [`AnnouncementService.cs`](file:///d:/onePj_oneMonth/Blood/BDMS_CSharp/BDMS.Domain/Features/Announcement/AnnouncementService.cs)

---

## GROUP 11 — Hospital Management (`api/hospital`)

CRUD for hospital entities. All staff and donors see hospital list.

| # | Method | Path | Permission |
|---|---|---|---|
| 53 | GET | /api/hospital/list | hospitals.view |
| 54 | GET | /api/hospital/get | hospitals.view |
| 55 | PUT | /api/hospital/update | hospitals.update |
| 56 | DELETE | /api/hospital/delete | hospitals.delete |

**Key files:**
- [`HospitalController.cs`](file:///d:/onePj_oneMonth/Blood/BDMS_CSharp/BDMS.Api/Controllers/HospitalController.cs)
- [`HospitalService.cs`](file:///d:/onePj_oneMonth/Blood/BDMS_CSharp/BDMS.Domain/Features/Hospital/HospitalService.cs)

---

## GROUP 12 — Medical Records (`api/medicalrecord`)

PHI: HIV, hepatitis B/C, malaria, syphilis screening results per donation. No per-donor scoping in queries.

| # | Method | Path | Permission |
|---|---|---|---|
| 57 | GET | /api/medicalrecord/list | medical_records.view |
| 58 | GET | /api/medicalrecord/{id} | medical_records.view |
| 59 | POST | /api/medicalrecord/create | medical_records.create |
| 60 | PUT | /api/medicalrecord/update | medical_records.update |
| 61 | DELETE | /api/medicalrecord/{id} | medical_records.delete |

**Key files:**
- [`MedicalRecordController.cs`](file:///d:/onePj_oneMonth/Blood/BDMS_CSharp/BDMS.Api/Controllers/MedicalRecordController.cs)
- [`MedicalRecordService.cs`](file:///d:/onePj_oneMonth/Blood/BDMS_CSharp/BDMS.Domain/Features/MedicalRecord/MedicalRecordService.cs)

---

## GROUP 13 — RBAC Administration (`api/permission`, `api/rolepermission`, `api/role`)

Manages the permission catalog and role-to-permission assignments. Privilege escalation surface: granting `permissions.manage` allows modifying who has access to anything.

| # | Method | Path | Permission |
|---|---|---|---|
| 62 | GET | /api/permission/list | permissions.view |
| 63 | POST | /api/permission/create | permissions.manage |
| 64 | PUT | /api/permission/edit | permissions.manage |
| 65 | DELETE | /api/permission/delete | permissions.manage |
| 66 | GET | /api/rolepermission/list | permissions.view |
| 67 | POST | /api/rolepermission/create | permissions.manage |
| 68 | PUT | /api/rolepermission/edit | permissions.manage |
| 69 | DELETE | /api/rolepermission/delete | permissions.manage |
| 70 | GET | /api/role/list | roles.view |
| 71 | POST | /api/role/create | roles.create |
| 72 | PUT | /api/role/edit | roles.update |
| 73 | DELETE | /api/role/delete | roles.delete |

**Key files:**
- [`PermissionController.cs`](file:///d:/onePj_oneMonth/Blood/BDMS_CSharp/BDMS.Api/Controllers/PermissionController.cs)
- [`RolePermissionController.cs`](file:///d:/onePj_oneMonth/Blood/BDMS_CSharp/BDMS.Api/Controllers/RolePermissionController.cs)
- [`RoleController.cs`](file:///d:/onePj_oneMonth/Blood/BDMS_CSharp/BDMS.Api/Controllers/RoleController.cs)

---

## GROUP 14 — User Account Management (`api/user`)

Admin-side CRUD for all user accounts. Includes role assignment, hospital assignment, and status (enable/disable).

| # | Method | Path | Permission |
|---|---|---|---|
| 74 | GET | /api/user/list | users.view |
| 75 | GET | /api/user/{id} | users.view |
| 76 | POST | /api/user/create | users.create |
| 77 | PUT | /api/user/update | users.update |
| 78 | PATCH | /api/user/{id}/status | users.update |
| 79 | DELETE | /api/user/delete/{userId} | users.delete |

**Key files:**
- [`UserController.cs`](file:///d:/onePj_oneMonth/Blood/BDMS_CSharp/BDMS.Api/Controllers/UserController.cs)
- [`UserService.cs`](file:///d:/onePj_oneMonth/Blood/BDMS_CSharp/BDMS.Domain/Features/User/UserService.cs)
