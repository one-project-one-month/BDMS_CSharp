#!/usr/bin/env python3
"""BDMS E2E — auth lifecycle: register/login/me/refresh/logout (client) + admin login/me/logout.
Cookie flow tested by hand-capturing Set-Cookie (Secure cookies won't round-trip over http via urllib)."""
import json, time, urllib.request, urllib.error, sys, re, uuid

BASE = "http://localhost:5017"
log, checks = [], []

def call(method, path, body=None, token=None, cookie=None):
    payload = json.dumps(body) if body is not None else None
    req = urllib.request.Request(BASE + path, method=method)
    req.add_header("Content-Type", "application/json")
    if token:
        req.add_header("Authorization", "Bearer " + token)
    if cookie:
        req.add_header("Cookie", cookie)
    data = payload.encode() if payload else None
    t0 = time.perf_counter()
    try:
        with urllib.request.urlopen(req, data) as r:
            raw = r.read().decode(); code = r.status; hdrs = r.headers
    except urllib.error.HTTPError as e:
        raw = e.read().decode(); code = e.code; hdrs = e.headers
    ms = (time.perf_counter() - t0) * 1000
    try:
        parsed = json.loads(raw)
    except Exception:
        parsed = {"_raw": raw[:250]}
    ok = 200 <= code < 300 and parsed.get("isSuccess") is not False
    note = (parsed.get("message") or (parsed.get("_raw") or ""))[:90]
    # multiple Set-Cookie headers — dict() would keep only one
    set_cookie = "\n".join(hdrs.get_all("Set-Cookie") or [])
    log.append((method, path, payload, code, ok, round(ms, 1), note))
    return code, parsed, set_cookie

def check(name, ok, detail=""):
    checks.append((name, ok, detail))
    print(("PASS" if ok else "FAIL"), "|", name, "|", detail)

def cookie_name_value(set_cookie, name):
    m = re.search(re.escape(name) + r"=([^;]*)", set_cookie or "")
    return m.group(1) if m else None

# ================= client: register
stamp = uuid.uuid4().hex[:8]
reg_email = f"e2e.auth.{stamp}@test.local"
c, r, sc = call("POST", "/api/userauth/register", {
    "userName": f"e2e_{stamp}", "email": reg_email,
    "password": "Passw0rd!123", "confirmPassword": "Passw0rd!123"})
reg_token = (r.get("data") or {}).get("token")
check("register: new user 200 + token", c == 200 and bool(reg_token),
      f"http={c} token={'yes' if reg_token else 'no'} msg={r.get('message')}")
client_cookie = cookie_name_value(sc, "BDMS_Client_Token")
check("register: sets BDMS_Client_Token cookie", bool(client_cookie),
      f"cookie={'yes' if client_cookie else 'no'}")

c, r, _ = call("POST", "/api/userauth/register", {
    "userName": "dup", "email": reg_email,
    "password": "Passw0rd!123", "confirmPassword": "Passw0rd!123"})
check("register: duplicate email rejected", c == 400 and "already" in str(r.get("message") or r.get("_raw")),
      f"http={c} msg={r.get('message') or r.get('_raw')}")

c, r, _ = call("POST", "/api/userauth/register", {
    "userName": "mismatch", "email": f"e2e.mis.{stamp}@test.local",
    "password": "Passw0rd!123", "confirmPassword": "Passw0rd!XYZ"})
check("register: password mismatch rejected", c == 400 and "match" in str(r.get("message") or r.get("_raw")).lower(),
      f"http={c} msg={r.get('message') or r.get('_raw')}")

# ================= client: login
c, r, sc = call("POST", "/api/userauth/login", {"email": reg_email, "password": "Passw0rd!123"})
login_token = (r.get("data") or {}).get("token")
check("login: correct credentials 200 + token", c == 200 and bool(login_token),
      f"http={c} token={'yes' if login_token else 'no'}")
login_cookie = cookie_name_value(sc, "BDMS_Client_Token") or client_cookie

c, r, _ = call("POST", "/api/userauth/login", {"email": reg_email, "password": "wrong-password"})
check("login: wrong password 401", c == 401, f"http={c} msg={r.get('message') or r.get('_raw')}")

c, r, _ = call("POST", "/api/userauth/login", {"email": "nobody@test.local", "password": "x"})
check("login: unknown email 401", c == 401, f"http={c} msg={r.get('message') or r.get('_raw')}")

# ================= me / authz
c, r, _ = call("GET", "/api/userauth/me", token=login_token)
check("me: valid token 200", c == 200 and r.get("isSuccess") is not False,
      f"http={c} email={(r.get('data') or {}).get('email')}")

c, r, _ = call("GET", "/api/userauth/me")
check("me: no token 401", c == 401, f"http={c}")

c, r, _ = call("GET", "/api/userauth/me", token="not.a.real.token")
check("me: garbage token 401", c == 401, f"http={c}")

# cookie path: hand-sent Cookie header
c, r, _ = call("GET", "/api/userauth/me", cookie=f"BDMS_Client_Token={login_cookie}")
check("me: client cookie accepted", c == 200, f"http={c}")

# ================= refresh
c, r, sc = call("POST", "/api/userauth/refresh-token", token=login_token)
refresh_token = (r.get("data") or {}).get("token")
check("refresh: 200 + new token", c == 200 and bool(refresh_token),
      f"http={c} rotated={'yes' if refresh_token and refresh_token != login_token else 'no/same'}")

c, r, _ = call("GET", "/api/userauth/me", token=refresh_token)
check("me: refreshed token works", c == 200, f"http={c}")

c, r, _ = call("POST", "/api/userauth/refresh-token")
check("refresh: no token 401", c == 401, f"http={c}")

# ================= client logout
c, r, sc = call("POST", "/api/userauth/logout", token=login_token)
check("logout: 200", c == 200, f"http={c} msg={r.get('message')}")
deleted = "BDMS_Client_Token=;" in (sc or "") or (sc or "").startswith("BDMS_Client_Token=;")
check("logout: clears client cookie", deleted, f"set-cookie={(sc or '')[:60]}")

# stateless JWT: old token still validates server-side (no denylist) — note, not a fail
c, r, _ = call("GET", "/api/userauth/me", token=login_token)
check("me: old token still valid after logout (stateless JWT)", c == 200,
      f"http={c} (expected 200 — logout only clears the cookie)")

# ================= admin side
c, r, sc = call("POST", "/api/auth/login", {"email": "admin@bdms.com", "password": "Admin@123"})
admin_token = (r.get("data") or {}).get("token")
admin_cookie = cookie_name_value(sc, "BDMS_Admin_Token")
check("admin login: 200 + token + cookie", c == 200 and bool(admin_token) and bool(admin_cookie),
      f"http={c} token={'yes' if admin_token else 'no'} cookie={'yes' if admin_cookie else 'no'}")

c, r, _ = call("POST", "/api/auth/login", {"email": "admin@bdms.com", "password": "nope"})
check("admin login: wrong password 401", c == 401, f"http={c} msg={r.get('message') or r.get('_raw')}")

c, r, _ = call("GET", "/api/auth/me", token=admin_token)
check("admin me: 200", c == 200, f"http={c}")

# donor token against admin me (role gate: admin/staff only)
c, r, _ = call("GET", "/api/auth/me", token=login_token)
check("admin me: donor token rejected 401", c == 401, f"http={c} msg={r.get('message') or r.get('_raw')}")

c, r, sc = call("POST", "/api/auth/logout", token=admin_token)
check("admin logout: 200 + clears admin cookie", c == 200 and "BDMS_Admin_Token=;" in (sc or ""),
      f"http={c} set-cookie={(sc or '')[:60]}")

# bearer still works after cookie logout (stateless)
c, r, _ = call("GET", "/api/auth/me", token=admin_token)
check("admin me: bearer still valid after logout (stateless)", c == 200, f"http={c}")

# login swaps cookies: client login clears admin cookie, admin login clears client cookie
c, r, sc = call("POST", "/api/userauth/login", {"email": reg_email, "password": "Passw0rd!123"})
check("client login: clears admin cookie", "BDMS_Admin_Token=;" in (sc or ""),
      f"set-cookie={(sc or '')[:80]}")
c, r, sc = call("POST", "/api/auth/login", {"email": "admin@bdms.com", "password": "Admin@123"})
check("admin login: clears client cookie", "BDMS_Client_Token=;" in (sc or ""),
      f"set-cookie={(sc or '')[:80]}")

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
