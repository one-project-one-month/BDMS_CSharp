# Stage 3 Coverage-Led Hunting — BDMS_CSharp

**Status:** source-only hunt complete for the assigned scope; candidates only. No candidate is confirmed. The installed Cloudflare-specific skill was not present in the available skill files; the repository was reviewed using the installed `security-audit` coverage-led hunting and attack-class guidance. The local execution controls required by that guidance are unavailable, so no build, test, process, or dynamic request was run.

## Coverage summary

Coverage is organized by affected boundary and workflow. Existing planned ledger units are retained and updated; new units cover the requested classes. Stage 3 assignments use only the relevant classes in `ATTACK-CLASSES.md`. Upload, webhook, and payment classes were checked and had no implemented surface in this repository. No live environment was accessed.

## Candidate set

### C-01 — Donor read permission exposes other donor profiles

- **Principal and action:** authenticated donor role calls donor list or detail endpoints.
- **Boundary/resource:** donor self-service permission to other donors' profile PII.
- **Source trace:** donor role receives `donors.view` in `BDMS.Database/InMemoryDbSeeder.cs:73-77`; endpoints require this permission in `BDMS.Api/Controllers/DonorController.cs:23,62`; `DonorService.GetAllDonors` selects the donor collection without `UserId` scoping (`BDMS.Domain/Features/Donor/DonorService.cs:20-24`), and `GetDonorById` selects by caller-supplied donor ID (`DonorService.cs:134-142`).
- **Candidate hypothesis:** the self-service role can read donor profiles belonging to other users.
- **Validation blockers:** production role-permission assignments may differ from the source seeder; no isolated dummy-account/API sandbox was available to observe the returned records. Keep as candidate.

### C-02 — Donor permissions expose medical screening records

- **Principal and action:** authenticated donor with `medical_records.view` reads medical-record list/detail endpoints.
- **Boundary/resource:** donor's own record boundary versus other donors' sensitive screening results.
- **Source trace:** donor grant is seeded at `BDMS.Database/InMemoryDbSeeder.cs:76-77`; routes require the permission at `BDMS.Api/Controllers/MedicalRecordController.cs:22`; `MedicalRecordService.GetAll` reads the full active record set without joining to the caller's donor identity (`BDMS.Domain/Features/MedicalRecord/MedicalRecordService.cs:17-24`).
- **Candidate hypothesis:** a donor role may read other donors' medical-screening data.
- **Validation blockers:** production role grants and the exact response contents require owner-observed/local dummy validation. Keep as candidate.

### C-03 — Donor appointment actions are not bound to appointment ownership

- **Principal and action:** donor role lists, reads, or updates appointments by endpoint/query IDs; donor role can also create an appointment using a donation ID.
- **Boundary/resource:** appointment owner (`Appointment.UserId`) versus a different donor's appointment. Completion and deletion require permissions the seeded donor role does not have and are excluded from this candidate.
- **Source trace:** donor grant includes `appointments.view/create/update` (`BDMS.Database/InMemoryDbSeeder.cs:76`); `AppointmentService.GetAllAppointments` returns rows without caller filtering (`BDMS.Domain/Features/Appointment/AppointmentService.cs:20-47`); update methods fetch by appointment ID alone (status path at lines 157-162; time path at lines 225-232); completion fetches by ID and may fulfill a blood request or complete a donation (`BDMS.Domain/Features/Appointment/AppointmentService.cs:267-394`).
- **Candidate hypothesis:** donor-granted appointment permissions may expose or mutate other principals' appointments and downstream workflow state.
- **Validation blockers:** mutable production grants and actual endpoint-level effects need a bounded local dummy-user check. Keep as candidate.

### C-04 — Donor donation permissions lack donor ownership binding

- **Principal and action:** donor role reads, creates, or updates donations using request-selected IDs and fields.
- **Boundary/resource:** donation belongs to the authenticated donor; persisted donor/hospital/status/approval fields.
- **Source trace:** donor receives donation view/create/update grants (`BDMS.Database/InMemoryDbSeeder.cs:75-76`); create command copies `DonorId`, `CreatedBy`, and other request fields (`BDMS.Api/Controllers/DonationController.cs:56-73`); `DonationService.CreateDonation` persists those values (`BDMS.Domain/Features/Donations/DonationService.cs:80-117`); update fetches by ID and assigns owner, hospital, status, and approval fields from the request (`BDMS.Domain/Features/Donations/DonationService.cs:174-205`).
- **Candidate hypothesis:** a donor may create or rewrite another donor's donation and alter workflow or audit fields.
- **Validation blockers:** production grants and request-to-persistence behavior need bounded local validation; no execution was performed. Keep as candidate.

### C-05 — User blood-request actions may cross the request owner's boundary

- **Principal and action:** user role lists/reads blood requests or creates a request with a supplied `UserId`.
- **Boundary/resource:** private request details and the identity attributed as request owner.
- **Source trace:** user grant includes `blood_requests.view/create` (`BDMS.Database/InMemoryDbSeeder.cs:85-88`); list/detail actions use those permissions (`BDMS.Api/Controllers/BloodRequestController.cs:24-48`); `BloodRequestService.GetAll` and `GetById` do not scope by caller (`BDMS.Domain/Features/BloodRequest/BloodRequestService.cs:33-58`); create takes `UserId` from the command and persists it (`BDMS.Domain/Features/BloodRequest/BloodRequestService.cs:90-108`; controller maps request model `UserId` at lines 47-56).
- **Candidate hypothesis:** a user may enumerate other requests or create a request attributed to another account.
- **Validation blockers:** production grants and a local dummy-user response/write check are unresolved. Keep as candidate.

### C-06 — Donation status update can bypass intended appointment completion

- **Principal and action:** a principal with `donations.approve` sets donation status directly to `completed`.
- **Boundary/resource:** allowed donation state transitions and exactly-once inventory effects.
- **Source trace:** endpoint accepts enum status and requires `donations.approve` (`BDMS.Api/Controllers/DonationController.cs:127-139`); service assigns the requested status and handles inventory on transition (`BDMS.Domain/Features/Donations/DonationService.cs:309-344`). Repository guidance says appointment completion is the intended completion route, while this endpoint permits direct completion.
- **Candidate hypothesis:** bypassing appointment completion may allow a donation to reach completed state and create inventory without the intended prerequisite checks.
- **Validation blockers:** stage 3 source evidence shows the alternate state path; an isolated test is needed to establish concrete business impact and any later guard. Keep as candidate.

### C-07 — Donor update can reassign a donor profile

- **Principal and action:** a principal with `donors.update` submits another `UserId` for a donor record.
- **Boundary/resource:** integrity of donor-to-account ownership mapping.
- **Source trace:** update endpoint copies `reqModel.UserId` into its command (`BDMS.Api/Controllers/DonorController.cs:78-101`); `DonorService.UpdateDonor` assigns `donor.UserId = reqModel.UserId` after selecting by donor ID (`BDMS.Domain/Features/Donor/DonorService.cs:169-194`).
- **Candidate hypothesis:** a staff principal with the seeded update grant may transfer a donor profile to another active identity or alter the identity association.
- **Validation blockers:** business intent for staff reassignment and production grants need owner confirmation; no local policy test was run. Keep as candidate.

### C-08 — Dashboard userId is accepted without a visible owner check

- **Principal and action:** any authenticated non-admin calls `GET /api/dashboard/{userId}` with a selected ID.
- **Boundary/resource:** user-specific dashboard data.
- **Source trace:** `[Authorize]` is the only action attribute and the route value is forwarded (`BDMS.Api/Controllers/DashboardController.cs:19-25`); MSSQL invokes `sp_GetDashboardData` with `user_id` (`BDMS.Domain/Features/Dashboard/DashboardService.cs:96-104`); InMemory mode returns global aggregates while ignoring the ID (`BDMS.Domain/Features/Dashboard/DashboardService.cs:29-86`).
- **Candidate hypothesis:** caller-controlled `userId` may reveal another user's dashboard data or global data.
- **Validation blockers:** stored-procedure filtering/authorization is outside source; InMemory behavior is not role-specific. Require local dummy-user confirmation of actual endpoint response. Keep as candidate.

### C-09 — Committed signing/database secrets may be usable outside development

- **Principal and action:** repository reader or actor with access to deployed configuration obtains committed JWT signing and database credentials.
- **Boundary/resource:** token issuer trust and database account authority.
- **Source trace:** committed settings contain these values at `BDMS.Api/appsettings.json:11,14`; application JWT validation uses configured signing key (`BDMS.Domain/FeatureManager.cs:89-102`). Values are intentionally not copied into this report.
- **Candidate hypothesis:** if still live and deployed, disclosure may enable token forgery or database access.
- **Validation blockers:** secret validity, rotation, runtime configuration precedence, and database/network reachability cannot be established from source. Keep as candidate; do not test or use the credentials.

### C-10 — Exception details may reach API callers

- **Principal and action:** caller triggers a service error and receives the result through a controller response.
- **Boundary/resource:** internal database/schema/connection details returned beyond the service boundary.
- **Source trace:** services interpolate `Exception.Message` into `SystemError`, e.g. `BDMS.Domain/Features/BloodRequest/BloodRequestService.cs:44-47` and `BDMS.Domain/Features/Donations/DonationService.cs:73-76`; dashboard also supplies exception message (`BDMS.Domain/Features/Dashboard/DashboardService.cs:107-110`).
- **Candidate hypothesis:** provider exceptions may disclose implementation or deployment details.
- **Validation blockers:** actual exception contents, API serialization, and environment behavior need a safe local failure fixture. Keep as candidate.

### C-11 — Credentialed cookie mutations may lack an effective CSRF control

- **Principal and action:** a hostile web origin causes a browser carrying an ambient API cookie to submit a state-changing request.
- **Boundary/resource:** authenticated account and blood-management state.
- **Source trace:** cookie is accepted by JWT middleware (`BDMS.Domain/FeatureManager.cs:105-117`); cookies are issued with `SameSite=None` (`BDMS.Api/Controllers/AuthController.cs:78-85`, `UserAuthController.cs:117-124`); CORS allows credentials for one exact origin (`BDMS.Api/Program.cs:24-32`); no anti-forgery or Origin validation was found on mutations.
- **Candidate hypothesis:** a browser-compatible simple request may cause state change even where CORS prevents reading the response.
- **Validation blockers:** browser cookie policy, route content-type/model-binding acceptance, and realistic hostile-origin request need an isolated local browser test. Keep as candidate.

### C-12 — Concurrent request fulfillment may allocate the same stock twice

- **Principal and action:** two authorized requests race donation completion or blood-request fulfillment against the same records/stock.
- **Boundary/resource:** unique donation inventory and inventory units allocated once.
- **Source trace:** blood-request fulfillment reads available inventory, checks count, then marks units used and saves (`BDMS.Domain/Features/BloodRequest/BloodRequestService.cs:221-255`). Appointment completion performs a similar read/adjust/save sequence (`BDMS.Domain/Features/Appointment/AppointmentService.cs:293-356`). A unique index prevents more than one inventory row for a donation (`BDMS.Database/AppDbContextModels/AppDbContext.cs:125`).
- **Candidate hypothesis:** concurrent fulfillment of separate requests may both select the same available unit before either save, leaving request/inventory state inconsistent.
- **Validation blockers:** transaction/isolation behavior and deterministic concurrent result are not established; source-only race is insufficient to confirm. Require a bounded local concurrency test with dummy state. Duplicate inventory for one donation is not alleged because of the unique index. Keep as candidate.

### C-13 — Concurrent donation appointment creation may duplicate bookings

- **Principal and action:** donor role submits simultaneous appointment requests for the same approved donation.
- **Boundary/resource:** one active appointment per donation.
- **Source trace:** `AppointmentService` checks `AnyAsync` for an open appointment and then inserts/saves in separate operations (`BDMS.Domain/Features/Appointment/AppointmentService.cs:112-134`); route requires `appointments.create` (`BDMS.Api/Controllers/AppointmentController.cs:59-68`); donor role receives that permission in `BDMS.Database/InMemoryDbSeeder.cs:72-81`.
- **Candidate hypothesis:** concurrent calls may pass the check before either insert. No unique donation-appointment index was found in the inspected mapping.
- **Validation blockers:** database constraints and actual concurrent outcome require a bounded local test with dummy rows. Keep as candidate.

### C-14 — Donation code generation uses only a process-local lock

- **Principal and action:** authorized donation creation requests execute on separate API instances for the same hospital and blood group.
- **Boundary/resource:** uniqueness of generated donation codes.
- **Source trace:** a static in-process semaphore serializes code generation within one process (`BDMS.Domain/Features/Donations/DonationService.cs:16,140-147`); the code is generated from the current maximum and then inserted (`DonationService.cs:147-163,80-115`).
- **Candidate hypothesis:** concurrent instances may compute the same next code if the database has no uniqueness constraint.
- **Validation blockers:** deployment instance count and database unique constraints are not established. Keep as candidate pending source/schema or owner evidence.

### C-15 — Public authentication responses disclose account existence

- **Principal and action:** anonymous caller submits candidate email addresses to client login or registration.
- **Boundary/resource:** privacy of registered account identities.
- **Source trace:** client login returns `Invalid email or password` for missing accounts but `Invalid password` for an existing account (`BDMS.Domain/Features/UserAuth/UserAuthService.cs:38-45`); registration returns `Email already exists` for a registered address (`UserAuthService.cs:118-122`); controllers return the service result on failure (`BDMS.Api/Controllers/UserAuthController.cs:33-40,59-64`).
- **Candidate hypothesis:** response text can distinguish registered addresses from unregistered ones.
- **Validation blockers:** response serialization/status behavior should be checked in a bounded local fixture. Keep as candidate; do not perform live enumeration.

### C-16 — Donor certificate permission may expose certificates across accounts

- **Principal and action:** donor role lists certificates or selects another donor ID for certificate lookup.
- **Boundary/resource:** certificates associated with other users.
- **Source trace:** donor role has `certificates.view` in the seeded grants (`BDMS.Database/InMemoryDbSeeder.cs:77`); list and donor lookup require that permission (`BDMS.Api/Controllers/CertificateController.cs:37-64`); `CertificateService.GetAllCertificates` returns all certificates and `GetCertificatesByDonorId` resolves records using the supplied donor ID rather than the authenticated identity (`BDMS.Domain/Features/Certificate/CertificateService.cs:70-83,122-148`).
- **Candidate hypothesis:** donor role may retrieve another account's certificate data.
- **Validation blockers:** production grants and endpoint responses need a bounded local dummy-user check. Keep as candidate.

## Reviewed and not promoted to candidates

- **Injection:** no string-built SQL sink was found in the searched services; observed EF queries are expression-based and the dashboard stored-procedure call binds `user_id` as a Dapper parameter. Continue checking any newly discovered raw-query path in later stages.
- **Privilege escalation:** permission/role management is gated by permission checks and seeded donor/user grants do not include management permissions. The admin bypass is intentional; no source-only escalation candidate survived this pass.
- **Rate limiting:** no limiter surfaced in the API wiring. Missing throttling alone did not establish account compromise or a concrete service boundary result; recorded as hardening, not a candidate.
- **Uploads, webhooks, payments:** no corresponding implemented surfaces found.

## Coverage limits and next-stage gate

This is not a confirmation report. Stored procedure internals, production role-permission rows, live-secret validity, deployment origin/browser behavior, database isolation/constraints, and runtime error serialization remain outside the evidence captured here. Every candidate requires independent Stage 4 validation; none is assigned severity or final verdict here.
