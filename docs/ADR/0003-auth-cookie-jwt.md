# ADR-0003: ASP.NET Core Identity + JWT in an httpOnly cookie

## Status

Accepted

## Problem

Secure authentication for a Next.js frontend talking to a separate ASP.NET
Core API, resistant to XSS (the platform treats security as a first-class
requirement), while staying simple for the MVP.

## Decision

Use ASP.NET Core Identity (Guid keys, PBKDF2 password hashing, built-in
lockout) and issue JWT access tokens that are delivered to the browser as an
**httpOnly, SameSite=Lax** cookie. The API validates the cookie in
`JwtBearerEvents.OnMessageReceived`; the `Authorization` bearer header is also
accepted for future non-browser API clients. No refresh token in Stage 1.

## Reason

- The token never lives in `localStorage`, so XSS cannot exfiltrate it.
- Works across dev ports: `localhost:3000` and `localhost:5050` are the same
  site, so the Lax cookie is sent on same-site requests.
- Identity gives password hashing and brute-force lockout for free.

## Alternatives

- JWT in `localStorage` — rejected: XSS risk, despite simpler CSRF story.
- OAuth/OIDC external providers — future stage (Google/Microsoft/Apple).
- Refresh tokens — deferred; short-lived access tokens + re-login for MVP.

## Trade-offs

- CSRF requires care (SameSite=Lax mitigates; explicit CSRF protection can be
  added later).
- Cookie delivery needs `credentials: "include"` and CORS with credentials.
- Stateless logout means logout is client-side cookie clearing for now.