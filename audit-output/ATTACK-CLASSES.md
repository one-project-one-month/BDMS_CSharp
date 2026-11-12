# Stage 3 Attack-Class Scope — BDMS_CSharp

Source basis: ASP.NET Core 8 REST API, JWT bearer/cookie authentication, custom permission filter, EF Core/SQL Server data services, and the donation, appointment, blood-request, inventory, donor, medical-record, certificate, and RBAC features. This class list is limited to the surfaces found in this repository. Findings remain candidates until Stage 4 validation.

## In scope

| Class | Relevant boundaries and entry paths |
|---|---|
| Authentication | Anonymous admin/client login and registration; JWT issue, claim binding, cookie handling, token refresh; `AuthService`, `UserAuthService`, `TokenService`, `DevCode`. |
| Authorization, BOLA, IDOR | Permission filter and all controller actions; per-record ownership for donors, donations, appointments, requests, medical records, certificates, inventory, and dashboard. Authentication middleware alone does not settle object authorization. |
| Privilege escalation | Admin bypass, role and permission CRUD, role-permission assignment, donor role promotion, staff and donor permissions. |
| Mass assignment | Request DTO and command fields such as `UserId`, `DonorId`, `CreatedBy`, `RoleId`, `HospitalId`, status, ownership, and approval fields; trace to persisted entities. |
| Injection | Request-derived values into EF queries, Dapper, stored procedure calls, logs, and error responses. EF LINQ parameterization and parameterized procedure calls are explicit controls to check. |
| Donation business logic | Donation status transitions, approval/completion paths, inventory creation, and duplicate or out-of-order effects. No payment feature is present in the discovered API. |
| Race conditions | Donation/request code generation, status transitions, inventory creation, stock consumption, and appointment completion; inspect check-then-write and uniqueness/transaction controls. |
| CORS and CSRF | Exact allowed origin, credentialed CORS, cookie flags, and every cookie-authenticated mutation. CORS response policy alone is not a CSRF control. |
| Rate limiting | Anonymous login and registration; absence is tracked as hardening unless a concrete account or service boundary impact is established. |
| Secrets | Committed configuration, signing key and database credentials, token claims, and any secret in logs/errors/responses. Whether a configured secret is live is outside source unless locally established. |
| Error handling | Exceptions returned through `Result`/controller responses; distinguish internal diagnostic details from generic errors and require an externally observable security impact. |

## Checked for applicability; no surface found

- **Uploads:** no upload route, multipart handler, or `IFormFile` processing found.
- **Webhooks:** no webhook route, callback delivery, or inbound webhook verifier found.
- **Payments:** no payment endpoint or payment service found; donation operations are blood-donation workflows.

These are not added as hunting units because no implemented surface was found. A future code path may change applicability.

## Evidence rules

- Keep every issue in candidate state. Stage 3 does not confirm severity or final verdict.
- A missing best practice alone is not a candidate. State the lower-trust principal, accepted action/value, intended control, affected resource/principal, source trace, and concrete consequence hypothesis.
- If a required deployment, browser, production role-grant, stored-procedure, or local behavior fact is not source-visible, list it as an explicit validation blocker.
- No deployed endpoint, external service, live credential, or shared environment is probed. Target-controlled execution is omitted because this session does not provide the required isolated OS sandbox.
