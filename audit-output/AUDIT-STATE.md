# AUDIT STATE — BDMS_CSharp

**Repo:** d:\onePj_oneMonth\Blood\BDMS_CSharp  
**Source ref:** f3448db (HEAD, branch Development-v1) — worktree dirty (`BDMS.Api/appsettings.Development.json` was modified before this Stage 3 run; audit outputs are also untracked)  
**Audit started:** 2026-11-11  
**Profile:** standard  
**Current stage:** 3 — Coverage-led, candidate-only hunting (complete)  
**Next stage:** 4 — Candidate validation (awaiting instruction)

---

## Files reviewed (Stage 1)

| File | Lines read |
|---|---|
| SPEC.md | 1–145 |
| CLAUDE.md | 1–61 |
| BDMS.Api/Program.cs | 1–138 |
| BDMS.Api/Filters/PermissionAttribute.cs | 1–77 |
| BDMS.Domain/FeatureManager.cs | 1–126 |
| BDMS.Shared/BaseController.cs | 1–27 |
| BDMS.Shared/DevCode.cs | 1–115 |
| BDMS.Shared/JwtSettings.cs | 1–19 |
| BDMS.Shared/Service/TokenService.cs | 1–61 |
| BDMS.Api/appsettings.json | 1–21 |
| BDMS.Api/appsettings.Development.json | 1–24 |
| BDMS.Api/Controllers/AuthController.cs | 1–87 |
| BDMS.Api/Controllers/UserAuthController.cs | 1–126 |
| BDMS.Api/Controllers/DonationController.cs | 1–164 |
| BDMS.Api/Controllers/AppointmentController.cs | 1–149 |
| BDMS.Api/Controllers/BloodRequestController.cs | 1–141 |
| BDMS.Api/Controllers/BloodInventoryController.cs | 1–99 |
| BDMS.Api/Controllers/DonorController.cs | 1–137 |
| BDMS.Api/Controllers/DashboardController.cs | 1–33 |
| BDMS.Api/Controllers/CertificateController.cs | 1–76 |
| BDMS.Api/Controllers/AnnouncementController.cs | 1–86 |
| BDMS.Api/Controllers/HospitalController.cs | 1–76 |
| BDMS.Api/Controllers/MedicalRecordController.cs | 1–108 |
| BDMS.Api/Controllers/PermissionController.cs | 1–73 |
| BDMS.Api/Controllers/RoleController.cs | 1–73 |
| BDMS.Api/Controllers/RolePermissionController.cs | 1–81 |
| BDMS.Api/Controllers/UserController.cs | 1–112 |
| BDMS.Database/AppDbContextModels/AppDbContext.cs | 1–560 |

---

## Routes found: 79 endpoints across 16 controllers

## Open questions / unresolved items (Stage 1)

1. **PermissionFilter missing claim check (step 2):** SPEC.md §2 says step 2 is "JWT has a matching Permission claim." PermissionFilter.cs L30–75 goes directly from admin-bypass (L41) to live DB lookup (L48–65), **skipping the in-token Permission claim check entirely.** The claim is added to the JWT by TokenService.cs L39, but PermissionFilter never reads it. SPEC declares it; code omits it. This is a discrepancy to investigate in Stage 2 (access-control class).
2. **`EncryptionHelper` is base64, not encryption:** DevCode.cs L100–110 — `Encrypt`/`Decrypt` are plain base64 encode/decode with no key, no cipher. The method name implies protection; the implementation offers none. UserIds and HospitalIds in JWT claims are trivially decoded. Flagged for Stage 2 investigation.
3. **Static password salt:** `PasswordHasherKeys.Salt` (DevCode.cs L95) is a fixed UTF-8 string "BDMS_STATIC_SALT" — identical for every user. No per-user random salt is generated or stored. Flagged for Stage 2.
4. **DashboardController IDOR candidate:** `GET /api/dashboard/{userId}` (DashboardController.cs L19–31) accepts any userId from the URL, no ownership check against the caller's identity. Flagged for Stage 2.
5. **DonorController `GetDonorById` route/param mismatch:** Route is `[HttpGet("{id}")]` but method parameter is `donorId` (no `[FromRoute]`). ASP.NET Core cannot bind `{id}` to `donorId` by default — this endpoint may always receive a default (0) donor ID. Flagged as incomplete trace.
6. **Swagger/OpenAPI exposed in production path:** `app.UseSwagger()` and `app.UseSwaggerUI()` are called unconditionally in Program.cs L91–92 — not gated by `IsDevelopment()`. Potential info-disclosure in production.
7. **appsettings.json contains live remote DB credentials and JWT key in source:** `appsettings.json` lines 11, 14 — a somee.com connection string with password and a fixed JWT signing key are committed to the repository.
8. **Service-layer and handler coverage:** Stage 3 traced selected ownership and workflow services, but did not independently trace every endpoint through every handler/service. Remaining units must stay visible for later coverage; do not infer full endpoint coverage from the candidate list.
9. **No OpenAPI spec file found:** No `.json`/`.yaml` OpenAPI spec exists in the repo; Swagger is generated dynamically. Route-source reconciliation relies on controller code vs. SPEC.md only.
10. **Testing coverage for RBAC:** Test files (Testing/*.cs) not yet read to reconcile against code findings.

---

## Stage completion checklist

- [x] Stage 1: Recon + endpoint discovery
- [x] Stage 2: Feature grouping and independent role assessments
- [x] Stage 3: Coverage-led candidate hunting
- [ ] Stage 4: Candidate validation
- [ ] Stage 5: Structured output (findings.json)
- [ ] Stage 6: Independent record verification
- [ ] Stage 7: Final report

## Stage 2 outputs

- 79 endpoints grouped in `feature-endpoint-groups.md` by implemented controller/domain feature.
- `role-permission-matrix.md`: 237 endpoint-role cells; 237 completed, 0 unresolved; real coverage 100.00% (completed / all cells).
- Donor/User role grants use the source seeder as baseline; production `Role_Permissions` rows remain mutable.

## Stage 3 outputs

- `HUNTING.md` contains 16 unconfirmed candidate hypotheses; none is confirmed or assigned severity.
- `coverage-ledger.json` contains 29 units: 18 candidate units mapping to 16 distinct fingerprints, source-covered units, and upload/webhook plus unrelated Swagger disclosure units marked out of scope.
- Hunting was source-only. The required isolated execution sandbox was unavailable; runtime, browser, stored-procedure, concurrency, and live-secret facts remain blockers where listed.
- Injection was searched across SQL entry points; payment, upload, and webhook paths were checked for applicability. Rate-limit absence remains hardening unless a concrete impact is established.
