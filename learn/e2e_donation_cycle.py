#!/usr/bin/env python3
"""BDMS E2E timed run — records endpoint, payload, status, latency for each call."""
import json, time, urllib.request, urllib.error, sys

BASE = "http://localhost:5017"
log = []  # (method, path, payload, http, ok, ms, note)

def call(method, path, body=None, token=None, label=""):
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
    ok = 200 <= code < 300 and (parsed.get("isSuccess") is not False)
    note = (parsed.get("message") or (parsed.get("_raw") or ""))[:80]
    log.append((method, path, payload, code, ok, round(ms, 1), note))
    return code, parsed

# 1. logins
c, r = call("POST", "/api/auth/login", {"email": "admin@bdms.com", "password": "Admin@123"})
admin = (r.get("data") or {}).get("token")
c, r = call("POST", "/api/userauth/login", {"email": "somchai.j@email.com", "password": "admin@123"})
donor = (r.get("data") or {}).get("token")
donor_uid = ((r.get("data") or {}).get("userInfo") or {}).get("userId")
if not admin or not donor:
    print("ABORT: login failed"); sys.exit(1)

call("GET", "/api/userauth/me", token=donor)
call("GET", "/api/auth/me", token=admin)

# 2. lookups
c, r = call("GET", "/api/hospital/list", token=admin)
hid = (r.get("data") or [{}])[0].get("id")
c, r = call("GET", "/api/donor/list", token=admin)
prof = next((d for d in (r.get("data") or []) if (d.get("userId") or d.get("UserId")) == donor_uid), {})
donor_pid = prof.get("id") or prof.get("Id")

# 3. donation lifecycle
c, r = call("POST", "/api/donation/Create", {
    "donorId": donor_pid, "hospitalId": hid, "createdBy": 1,
    "bloodGroup": "O+", "unitsDonated": 2, "remarks": "e2e timed"}, token=admin)
donation_id = (r.get("data") or {}).get("id")
if not donation_id:
    print("ABORT: donation create failed"); sys.exit(1)

# 4. medical record
c, r = call("POST", "/api/medicalrecord/create", {
    "donationId": donation_id, "hospitalId": hid, "hemoglobinLevel": 13.5,
    "hivResult": "Negative", "hepatitisBResult": "Negative", "hepatitisCResult": "Negative",
    "malariaResult": "Negative", "syphilisResult": "Negative",
    "screeningStatus": "Passed", "screeningNotes": "e2e screening", "screenedBy": 1}, token=admin)
med_id = (r.get("data") or {}).get("id")
call("GET", "/api/medicalrecord/list", token=donor)

# 5. approve donation
for st in ("screening", "approved"):
    call("PATCH", "/api/donation/UpdateStatus", {"id": donation_id, "status": st}, token=admin)

# 6. appointment: donor first (expect 403), then admin
c, r = call("POST", f"/api/appointment/donation/{donation_id}", {"remarks": "e2e"}, token=donor)
if not r.get("isSuccess"):
    c, r = call("POST", f"/api/appointment/donation/{donation_id}", {"remarks": "e2e"}, token=admin)
appt_id = (r.get("data") or {}).get("id")
if not appt_id:
    print("ABORT: appointment create failed"); sys.exit(1)

# 7. confirm + complete
call("PATCH", f"/api/appointment/{appt_id}/status", {"status": "confirmed"}, token=admin)
call("POST", f"/api/appointment/{appt_id}/complete", token=admin)

# 8. verify
call("GET", f"/api/appointment/{appt_id}", token=admin)
c, r = call("GET", "/api/donation/List", token=admin)
mine = next((d for d in (r.get("data") or []) if (d.get("id") or d.get("Id")) == donation_id), {})
log.append(("CHECK", "donation status == completed", None, 200,
            str(mine.get("status", "")).lower() == "completed", 0, str(mine.get("status"))))

c, r = call("GET", f"/api/bloodinventory/hospital/{hid}", token=admin)
row = next((x for x in (r.get("data") or []) if (x.get("donationId") or x.get("DonationId")) == donation_id), None)
log.append(("CHECK", "inventory row for donation", None, 200, row is not None, 0,
            f"units={row and (row.get('units') or row.get('Units'))} status={row and (row.get('status') or row.get('Status'))}"))

call("GET", "/api/donation/List", token=donor)
call("GET", "/api/appointment/list", token=donor)
call("GET", "/api/bloodinventory/available-stock", token=donor)

# report
print("| # | Method | Endpoint | Payload | HTTP | OK | ms | Note |")
print("|---|--------|----------|---------|------|----|----|------|")
for i, (m, p, pl, code, ok, ms, note) in enumerate(log, 1):
    pl = (pl[:70] + "…") if pl and len(pl) > 70 else (pl or "—")
    pl = pl.replace("|", "\\|")
    note = (note or "").replace("|", "\\|")
    print(f"| {i} | {m} | {p} | {pl} | {code} | {'✓' if ok else '✗'} | {ms} | {note} |")

fails = [x for x in log if not x[4]]
total = sum(x[5] for x in log if x[0] != "CHECK")
print(f"\n{len(log)-len(fails)}/{len(log)} ok | total request time {round(total,1)} ms | "
      f"max {max(x[5] for x in log if x[0] != 'CHECK')} ms")
sys.exit(1 if fails else 0)
