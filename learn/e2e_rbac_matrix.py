#!/usr/bin/env python3
"""BDMS E2E — RBAC matrix: 4 roles × permission families against the live API.
Expectations come from InMemoryDbSeeder's Role_Permissions (documented intent).
Denied → 403; allowed → not 401/403 (reads should be 200; writes use safe/no-op payloads).
Model-binding 400s run BEFORE the permission filter, so write probes send valid-shaped bodies."""
import json, time, urllib.request, urllib.error, sys, uuid

BASE = "http://localhost:5017"
log, checks = [], []

def call(method, path, body=None, token=None):
    payload = json.dumps(body) if body is not None else None
    req = urllib.request.Request(BASE + path, method=method)
    req.add_header("Content-Type", "application/json")
    if token:
        req.add_header("Authorization", "Bearer " + token)
    data = payload.encode() if payload else None
    t0 = time.perf_counter()
    try:
        with urllib.request.urlopen(req, data) as r:
            raw = r.read().decode(); code = r.status
    except urllib.error.HTTPError as e:
        raw = e.read().decode(); code = e.code
    ms = (time.perf_counter() - t0) * 1000
    try:
        parsed = json.loads(raw)
    except Exception:
        parsed = {"_raw": raw[:200]}
    note = (parsed.get("message") or (parsed.get("_raw") or ""))[:80]
    log.append((method, path, token[:12] + "…" if token else "—", code, round(ms, 1), note))
    return code, parsed

def check(name, ok, detail=""):
    checks.append((name, ok, detail))
    print(("PASS" if ok else "FAIL"), "|", name, "|", detail)

# ---- logins
def login_user(email, pw, path="/api/userauth/login"):
    c, r = call("POST", path, {"email": email, "password": pw})
    return (r.get("data") or {}).get("token")

admin = login_user("admin@bdms.com", "Admin@123", "/api/auth/login")
staff = login_user("staff@bdms.com", "Staff@123")  # live-DB password; seeder's staff@123 is drift
donor = login_user("somchai.j@email.com", "admin@123")
if not admin or not staff or not donor:
    print("ABORT: login failed", {"admin": bool(admin), "staff": bool(staff), "donor": bool(donor)})
    sys.exit(2)

stamp = uuid.uuid4().hex[:8]
c, r = call("POST", "/api/userauth/register", {
    "userName": f"rbac_{stamp}", "email": f"e2e.rbac.{stamp}@test.local",
    "password": "Passw0rd!123", "confirmPassword": "Passw0rd!123"})
user = (r.get("data") or {}).get("token")
if not user:
    print("ABORT: register failed"); sys.exit(2)

TOKENS = {"admin": admin, "staff": staff, "donor": donor, "user": user}

# allowed-role sets from InMemoryDbSeeder
A = lambda *rs: set(rs)
ALLOW = {
    "donations.view":        A("admin","staff","donor"),
    "donations.create":      A("admin","staff","donor"),
    "donations.approve":     A("admin","staff"),
    "donations.delete":      A("admin"),
    "appointments.view":     A("admin","staff","donor","user"),
    "appointments.create":   A("admin","staff","donor","user"),
    "appointments.update":   A("admin","staff","donor","user"),
    "appointments.complete": A("admin","staff"),
    "appointments.delete":   A("admin"),
    "blood_requests.view":   A("admin","staff","user"),
    "blood_requests.create": A("admin","staff","user"),
    "blood_requests.approve":A("admin","staff"),
    "blood_requests.delete": A("admin"),
    "blood_inventories.view":  A("admin","staff"),
    "blood_inventories.create":A("admin","staff"),
    "blood_inventories.update":A("admin","staff"),
    "blood_inventories.delete":A("admin"),
    "donors.view":           A("admin","staff","donor"),
    "donors.create":         A("admin","staff"),
    "donors.update":         A("admin","staff"),
    "donors.delete":         A("admin"),
    "medical_records.view":  A("admin","staff","donor"),
    "medical_records.create":A("admin","staff"),
    "medical_records.delete":A("admin"),
    "announcements.view":    A("admin","staff","donor","user"),
    "announcements.create":  A("admin"),
    "announcements.delete":  A("admin"),
    "certificates.view":     A("admin","staff","donor"),
    "certificates.create":   A("admin","staff"),
    "hospitals.view":        A("admin","staff","user"),
    "hospitals.delete":      A("admin"),
    "users.view":            A("admin","staff"),
    "users.create":          A("admin"),
    "roles.view":            A("admin"),
    "roles.create":          A("admin"),
    "permissions.view":      A("admin"),
}

# probe = (perm, method, path, body) — body valid-shaped; FK ids are 999999 so
# an allowed call fails business validation instead of mutating data.
# admin_only_noop: allowed side not probed (would create junk) — only denials asserted.
PROBES = [
    ("donations.view",         "GET",  "/api/donation/List", None),
    ("donations.create",       "POST", "/api/donation/Create",
        {"donorId":999999,"hospitalId":999999,"createdBy":1,"bloodGroup":"O+","unitsDonated":1,"remarks":"rbac"}),
    ("donations.approve",      "PATCH","/api/donation/UpdateStatus", {"id":999999,"status":"approved"}),
    ("donations.delete",       "DELETE","/api/donation/Delete?DonationId=999999", None),
    ("appointments.view",      "GET",  "/api/appointment/list", None),
    ("appointments.create",    "POST", "/api/appointment/donation/999999", {"remarks":"rbac"}),
    ("appointments.update",    "PATCH","/api/appointment/999999/status", {"status":"confirmed"}),
    ("appointments.complete",  "POST", "/api/appointment/999999/complete", None),
    ("appointments.delete",    "DELETE","/api/appointment/999999", None),
    ("blood_requests.view",    "GET",  "/api/bloodrequest/list", None),
    ("blood_requests.create",  "POST", "/api/bloodrequest/create",
        {"userId":999999,"hospitalId":999999,"patientName":"RBAC","bloodGroup":"O+",
         "unitsRequired":1,"urgency":"high","requiredDate":"2026-11-20","reason":"rbac"}),
    ("blood_requests.approve", "PATCH","/api/bloodrequest/999999/status", {"status":"approved"}),
    ("blood_requests.delete",  "DELETE","/api/bloodrequest/999999", None),
    ("blood_inventories.view", "GET",  "/api/bloodinventory/available-stock", None),
    ("blood_inventories.create","POST","/api/bloodinventory/add/999999", None),
    ("blood_inventories.update","PATCH","/api/bloodinventory/use",
        {"bloodInventoryId":999999,"requestId":999999}),
    ("blood_inventories.delete","DELETE","/api/bloodinventory/999999", None),
    ("donors.view",            "GET",  "/api/donor/list", None),
    ("donors.create",          "POST", "/api/donor/create",
        {"userId":999999,"bloodGroup":"O+","nicNo":"RBAC-NOPE","isActive":True,
         "gender":"male","dateOfBirth":"1990-01-01"}),
    ("donors.delete",          "DELETE","/api/donor/delete?donorId=999999", None),
    ("medical_records.view",   "GET",  "/api/medicalrecord/list", None),
    ("medical_records.create", "POST", "/api/medicalrecord/create",
        {"donationId":999999,"hospitalId":999999,"bloodGroup":"O+","screeningResult":"fit"}),
    ("medical_records.delete", "DELETE","/api/medicalrecord/999999", None),
    ("announcements.view",     "GET",  "/api/announcement/List", None),
    ("announcements.create",   "POST", "/api/announcement",
        {"title":"RBAC probe — ignore","content":"probe","category":"News"}),
    ("announcements.delete",   "DELETE","/api/announcement/999999", None),
    ("certificates.view",      "GET",  "/api/certificate/list", None),
    ("certificates.create",    "POST", "/api/certificate/Generate",
        {"donationId":999999}),
    ("hospitals.view",         "GET",  "/api/hospital/list", None),
    ("hospitals.delete",       "DELETE","/api/hospital/delete?id=999999", None),
    ("users.view",             "GET",  "/api/user/list", None),
    ("users.create",           "POST", "/api/user/create",
        {"userName":"rbac_probe","email":f"rbac.{stamp}@x.local","password":"Passw0rd!123","roleId":1}),
    ("roles.view",             "GET",  "/api/role/list", None),
    ("roles.create",           "POST", "/api/role/create", {"name": f"rbac_{stamp}"}),
    ("permissions.view",       "GET",  "/api/permission/list", None),
]

# probes where the ALLOWED side would mutate if called — only assert denials
DENY_ONLY = {
    "announcements.create", "announcements.delete", "users.create", "roles.create",
}

# ---- unauthenticated baseline
c, _ = call("GET", "/api/donation/List")
check("anon: permission endpoint 401", c == 401, f"http={c}")
c, _ = call("GET", "/api/dashboard/1")
check("anon: dashboard 401", c == 401, f"http={c}")

# ---- dashboard: any authenticated role (Authorize only)
for role, tok in TOKENS.items():
    c, r = call("GET", "/api/dashboard/1", token=tok)
    check(f"dashboard: {role} allowed", c == 200, f"http={c}")

# ---- matrix
fail_cells = []
for perm, method, path, body in PROBES:
    allowed = ALLOW[perm]
    for role, tok in TOKENS.items():
        should_allow = role in allowed
        if not should_allow:
            c, r = call(method, path, body, token=tok)
            ok = (c == 403)
            name = f"{perm} | {role} → denied"
            if not ok:
                fail_cells.append((perm, role, c, r.get("message") or r.get("_raw")))
            check(name, ok, f"http={c} msg={(r.get('message') or r.get('_raw') or '')[:60]}")
        else:
            if perm in DENY_ONLY and role != "admin":
                continue  # shouldn't happen (allowed set is admin-only) — guard
            if perm in DENY_ONLY:
                continue  # skip admin side: would create junk; denial already asserted for others
            c, r = call(method, path, body, token=tok)
            ok = c not in (401, 403)
            # reads should succeed outright
            if method == "GET":
                ok = (200 <= c < 300)
            name = f"{perm} | {role} → allowed"
            if not ok:
                fail_cells.append((perm, role, c, r.get("message") or r.get("_raw")))
            check(name, ok, f"http={c} msg={(r.get('message') or r.get('_raw') or '')[:60]}")

# ---- report
print("\n--- Summary by permission ---")
by_perm = {}
for name, ok, detail in checks:
    if " | " in name:
        perm = name.split(" | ")[0]
        by_perm.setdefault(perm, []).append(ok)
for perm, oks in by_perm.items():
    print(f"{'OK ' if all(oks) else 'DRIFT'} | {perm} | {sum(oks)}/{len(oks)} cells")

print("\n--- HTTP log ---")
print("| # | Method | Endpoint | Token | HTTP | ms | Note |")
print("|---|--------|----------|-------|------|----|------|")
for i, (m, p, t, code, ms, note) in enumerate(log, 1):
    print(f"| {i} | {m} | {p} | {t} | {code} | {ms} | {(note or '').replace('|','\\\\|')} |")

fails = [x for x in checks if not x[1]]
total = len(log) and sum(x[4] for x in log) or 0
print(f"\n{len(checks)-len(fails)}/{len(checks)} checks passed | {len(log)} requests | {round(total,1)} ms total")
if fail_cells:
    print("\nDrift cells (live vs seeder intent):")
    for perm, role, code, msg in fail_cells:
        print(f"  {perm} × {role}: http={code} {str(msg)[:80]}")
sys.exit(1 if fails else 0)
