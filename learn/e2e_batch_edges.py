#!/usr/bin/env python3
"""BDMS E2E batch — edges: #4 completion idempotency, #1-3 blood request edges,
#6 appointment transitions, #7 inventory ops. Timed; text/plain errors parsed defensively."""
import json, time, urllib.request, urllib.error, sys

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
        parsed = {"_raw": raw[:250]}
    ok = 200 <= code < 300 and parsed.get("isSuccess") is not False
    note = (parsed.get("message") or (parsed.get("_raw") or ""))[:90]
    log.append((method, path, payload, code, ok, round(ms, 1), note))
    return code, parsed

def check(name, ok, detail=""):
    checks.append((name, ok, detail))
    print(("PASS" if ok else "FAIL"), "|", name, "|", detail)

def rows_for(token, hid, donation_id=None, request_id=None, available_only=False):
    c, r = call("GET", f"/api/bloodinventory/hospital/{hid}", token=token)
    rows = r.get("data") or []
    if donation_id is not None:
        rows = [x for x in rows if (x.get("donationId") or x.get("DonationId")) == donation_id]
    if request_id is not None:
        rows = [x for x in rows if (x.get("requestId") or x.get("RequestId")) == request_id]
    if available_only:
        rows = [x for x in rows if str(x.get("status", "")).lower() == "available"]
    return rows

# ---- logins + lookups
c, r = call("POST", "/api/auth/login", {"email": "admin@bdms.com", "password": "Admin@123"})
admin = (r.get("data") or {}).get("token")
c, r = call("POST", "/api/userauth/login", {"email": "somchai.j@email.com", "password": "admin@123"})
donor = (r.get("data") or {}).get("token")
if not admin or not donor:
    print("ABORT: login failed"); sys.exit(1)

c, r = call("GET", "/api/hospital/list", token=admin)
hid = (r.get("data") or [{}])[0].get("id")
c, r = call("GET", "/api/donor/list", token=admin)
prof = next(d for d in (r.get("data") or []) if (d.get("userId") or d.get("UserId")) == 3)
donor_pid = prof.get("id") or prof.get("Id")   # profile id = 1; O+ per DB

def mk_donation(remark):
    c, r = call("POST", "/api/donation/Create", {
        "donorId": donor_pid, "hospitalId": hid, "createdBy": 1,
        "bloodGroup": "O+", "unitsDonated": 1, "remarks": remark}, token=admin)
    return (r.get("data") or {}).get("id")

def mk_request(bg, units, tag):
    c, r = call("POST", "/api/bloodrequest/create", {
        "userId": 3, "hospitalId": hid, "patientName": f"E2E {tag}",
        "bloodGroup": bg, "unitsRequired": units, "urgency": "high",
        "requiredDate": "2026-11-20", "reason": tag}, token=admin)
    return (r.get("data") or {}).get("id")

def appt_for(req_id):
    c, r = call("GET", "/api/appointment/list", token=admin)
    a = next((x for x in (r.get("data") or [])
              if (x.get("bloodRequestId") or x.get("BloodRequestId")) == req_id), {})
    return a.get("id") or a.get("Id")

# ================= #4a: UpdateStatus -> completed, then idempotent re-run
d1 = mk_donation("batch-4a")
for st in ("screening", "approved", "completed"):
    call("PATCH", "/api/donation/UpdateStatus", {"id": d1, "status": st}, token=admin)
n = len(rows_for(admin, hid, donation_id=d1))
check("4a: UpdateStatus -> completed creates 1 inventory row", n == 1, f"donation={d1} rows={n}")

call("PATCH", "/api/donation/UpdateStatus", {"id": d1, "status": "completed"}, token=admin)
n = len(rows_for(admin, hid, donation_id=d1))
check("4a: second -> completed is idempotent (still 1 row)", n == 1, f"rows={n}")

# ================= #4b + #6: appointment path, bad transition, reschedule, cross-path idempotency, delete
d2 = mk_donation("batch-4b")
for st in ("screening", "approved"):
    call("PATCH", "/api/donation/UpdateStatus", {"id": d2, "status": st}, token=admin)
c, r = call("POST", f"/api/appointment/donation/{d2}", {"remarks": "batch"}, token=admin)
a2 = (r.get("data") or {}).get("id")

# 6b: reschedule while still Scheduled (API only allows it pre-confirm)
c, r = call("PATCH", f"/api/appointment/{a2}/time",
            {"appointmentDate": "2026-11-25", "appointmentTime": "14:30"}, token=admin)
c, r = call("GET", f"/api/appointment/{a2}", token=admin)
a = r.get("data") or {}
ok6b = (str(a.get("appointmentDate", "")).startswith("2026-11-25")
        and str(a.get("appointmentTime", "")).startswith("14:30"))
check("6b: reschedule via PATCH time (while Scheduled)", ok6b,
      f"date={a.get('appointmentDate')} time={a.get('appointmentTime')}")

call("PATCH", f"/api/appointment/{a2}/status", {"status": "confirmed"}, token=admin)

# 6a: confirmed -> completed via PATCH must fail
c, r = call("PATCH", f"/api/appointment/{a2}/status", {"status": "completed"}, token=admin)
check("6a: confirmed -> completed via PATCH rejected", not (200 <= c < 300), f"http={c} msg={r.get('message') or r.get('_raw')}")

# 6d: reschedule after confirm must fail
c, r = call("PATCH", f"/api/appointment/{a2}/time",
            {"appointmentDate": "2026-11-26", "appointmentTime": "15:00"}, token=admin)
check("6d: reschedule rejected once Confirmed", not (200 <= c < 300),
      f"http={c} msg={r.get('message') or r.get('_raw')}")

# complete via endpoint (valid path) + cross-path idempotency
call("POST", f"/api/appointment/{a2}/complete", token=admin)
n = len(rows_for(admin, hid, donation_id=d2))
check("4b: appointment complete creates 1 inventory row", n == 1, f"donation={d2} rows={n}")

c, r = call("PATCH", "/api/donation/UpdateStatus", {"id": d2, "status": "completed"}, token=admin)
n = len(rows_for(admin, hid, donation_id=d2))
ok4b = (200 <= c < 300 or r.get("isSuccess")) and n == 1
check("4b: UpdateStatus after /complete stays 1 row (cross-path idempotent)", ok4b, f"http={c} rows={n}")

# 6c: delete
c, r = call("DELETE", f"/api/appointment/{a2}", token=admin)
c, r = call("GET", f"/api/appointment/{a2}", token=admin)
check("6c: DELETE appointment then GET fails", not (200 <= c < 300 and r.get("isSuccess")),
      f"delete then get http={c} msg={r.get('message') or r.get('_raw')}")

# ================= #1: reject
r1 = mk_request("O+", 1, "reject")
call("PATCH", f"/api/bloodrequest/{r1}/status", {"status": "rejected"}, token=admin)
c, r = call("GET", f"/api/bloodrequest/{r1}", token=admin)
st = str((r.get("data") or {}).get("status", "")).lower()
check("1: reject path", st == "rejected", f"status={st}")
c, r = call("GET", "/api/appointment/list", token=admin)
open_a = [x for x in (r.get("data") or [])
          if (x.get("bloodRequestId") or x.get("BloodRequestId")) == r1
          and str(x.get("status", "")).lower() != "cancelled"]
check("1: rejected request has no open appointment", not open_a, f"open={len(open_a)}")

# ================= #2: direct fulfil + blood-group mismatch
c, r = call("GET", f"/api/bloodinventory/hospital/{hid}", token=admin)
before = sum((x.get("units") or 0) for x in (r.get("data") or [])
             if str(x.get("status", "")).lower() == "available"
             and str(x.get("bloodGroup", "")).upper() == "O+")
r2 = mk_request("O+", 1, "direct-fulfil")
c, r = call("PATCH", f"/api/bloodrequest/{r2}/status",
            {"status": "fulfilled", "donorId": donor_pid}, token=admin)
c, r2get = call("GET", f"/api/bloodrequest/{r2}", token=admin)
st = str((r2get.get("data") or {}).get("status", "")).lower()
c, rinv = call("GET", f"/api/bloodinventory/hospital/{hid}", token=admin)
after = sum((x.get("units") or 0) for x in (rinv.get("data") or [])
            if str(x.get("status", "")).lower() == "available"
            and str(x.get("bloodGroup", "")).upper() == "O+")
tagged = [x for x in (rinv.get("data") or [])
          if (x.get("requestId") or x.get("RequestId")) == r2]
check("2: direct fulfilled with donorId", st == "fulfilled", f"status={st} msg={r.get('message')}")
# shared consume decrements units; a multi-unit row may stay available (no whole-row waste)
check("2: exactly 1 unit consumed (partial row allowed)", after == before - 1,
      f"available before={before} after={after} tagged_rows={len(tagged)}")

r3 = mk_request("A+", 1, "mismatch")
c, r = call("PATCH", f"/api/bloodrequest/{r3}/status",
            {"status": "fulfilled", "donorId": donor_pid}, token=admin)
check("2: mismatched donor blood group rejected",
      not (200 <= c < 300), f"http={c} msg={r.get('message') or r.get('_raw')}")
call("DELETE", f"/api/bloodrequest/{r3}", token=admin)

# ================= #3: insufficient stock on /complete
r4 = mk_request("O+", 99, "insufficient")
call("PATCH", f"/api/bloodrequest/{r4}/status", {"status": "approved"}, token=admin)
a4 = appt_for(r4)
call("PATCH", f"/api/appointment/{a4}/status", {"status": "confirmed"}, token=admin)
c, r = call("POST", f"/api/appointment/{a4}/complete", token=admin)
msg = r.get("message") or r.get("_raw") or ""
check("3: insufficient stock fails complete", not (200 <= c < 300) and "Insufficient" in str(msg),
      f"http={c} msg={str(msg)[:100]}")
call("DELETE", f"/api/appointment/{a4}", token=admin)
call("PATCH", f"/api/bloodrequest/{r4}/status", {"status": "rejected"}, token=admin)

# ================= #7: inventory admin ops
c, r = call("GET", "/api/bloodinventory/available-stock", token=admin)
check("7: admin available-stock", c == 200 and r.get("isSuccess") is not False, f"http={c}")

c, r = call("POST", "/api/bloodinventory/stock-take?hospitalId=1", token=admin)
check("7: stock-take runs", 200 <= c < 300, f"http={c} msg={r.get('message') or r.get('_raw')}")

# delete D2's row, then re-add via add/{donationId}
row_d2 = rows_for(admin, hid, donation_id=d2)
inv_id = row_d2 and (row_d2[0].get("id") or row_d2[0].get("Id"))
c, r = call("DELETE", f"/api/bloodinventory/{inv_id}", token=admin)
gone = not rows_for(admin, hid, donation_id=d2)
check("7: DELETE inventory row", 200 <= c < 300 and gone, f"http={c} rows_left={0 if gone else 1}")

c, r = call("POST", f"/api/bloodinventory/add/{d2}", token=admin)
back = rows_for(admin, hid, donation_id=d2)
check("7: add/{donationId} recreates row", 200 <= c < 300 and len(back) == 1,
      f"http={c} rows={len(back)} msg={r.get('message') or r.get('_raw')}")

# use: mark an available O+ row as used against r2
avail = rows_for(admin, hid, available_only=True)
avail = [x for x in avail if str(x.get("bloodGroup", "")).upper() == "O+"]
use_id = avail and (avail[0].get("id") or avail[0].get("Id"))
c, r = call("PATCH", "/api/bloodinventory/use",
            {"bloodInventoryId": use_id, "requestId": r2}, token=admin)
used_row = rows_for(admin, hid, request_id=r2)
check("7: PATCH use tags row", 200 <= c < 300 and len(used_row) >= 1,
      f"http={c} tagged={len(used_row)} msg={r.get('message') or r.get('_raw')}")

# ---- report
print("\n--- HTTP log ---")
print("| # | Method | Endpoint | Payload | HTTP | OK | ms | Note |")
print("|---|--------|----------|---------|------|----|----|------|")
for i, (m, p, pl, code, ok, ms, note) in enumerate(log, 1):
    pl = (pl[:65] + "…") if pl and len(pl) > 65 else (pl or "—")
    print(f"| {i} | {m} | {p} | {pl.replace('|','\\\\|')} | {code} | {'✓' if ok else '✗'} | {ms} | {(note or '').replace('|','\\\\|')} |")

print("\n--- Checks ---")
for name, ok, detail in checks:
    print(("PASS" if ok else "FAIL"), "|", name, "|", detail)

fails = [x for x in checks if not x[1]]
total = sum(x[5] for x in log)
print(f"\n{len(checks)-len(fails)}/{len(checks)} checks passed | {len(log)} requests | {round(total,1)} ms total")
sys.exit(1 if fails else 0)
