# API

Base URL (dev): `http://localhost:5050`

## Response contract

### Success

```json
{
  "data": {},
  "meta": {}
}
```

`meta` is optional (e.g. pagination info) and omitted when empty.

### Error

```json
{
  "error": {
    "code": "CHEF_NOT_FOUND",
    "message": "Chef was not found."
  }
}
```

Error `code`s are stable machine-readable strings. Clients should switch on the
`code`, not the HTTP status text.

### Validation errors

Unprocessable model state returns `400` with code `VALIDATION_ERROR` and a
message listing the failing fields.

## HTTP status codes

| Code | Meaning |
| ---- | ------- |
| 200 | OK |
| 201 | Created |
| 204 | No content |
| 400 | Validation error / bad request |
| 401 | Unauthenticated / invalid credentials |
| 403 | Authenticated but not authorized |
| 404 | Resource not found |
| 409 | Conflict (e.g. duplicate email) |
| 500 | Internal error (details never leaked) |

## Endpoints

### System

| Method | Path | Description |
| ------ | ---- | ----------- |
| GET | `/health` | Liveness + readiness; includes DB health check |
| GET | `/openapi/v1.json` | OpenAPI document (dev only) |

### Authentication (Stage 1)

| Method | Path | Description |
| ------ | ---- | ----------- |
| POST | `/api/auth/register` | Register a customer or chef |
| POST | `/api/auth/login` | Log in (sets an httpOnly auth cookie) |
| POST | `/api/auth/logout` | Log out (clears the auth cookie) |
| GET | `/api/users/me` | Current user (requires auth) |

Authentication uses a JWT delivered as an httpOnly `HomeChef.Auth` cookie
(`SameSite=Lax`; `Secure` only when `RequireSecureCookie` is enabled). The API
also accepts the same token as a `Authorization: Bearer <token>` header. All
`/api/users/*` routes require authentication.

#### POST `/api/auth/register`

Self-service registration. `role` is optional and defaults to `Customer`;
only `Customer` and `Chef` can be self-assigned. Returns the user and sets the
auth cookie (signs the user in).

```json
// request
{
  "firstName": "Mina",
  "lastName": "Khan",
  "email": "mina@example.com",
  "password": "Password123",
  "role": "Chef"
}

// 201 response
{
  "data": {
    "id": "353c4c46-fc86-4b16-b2aa-fd8c46c573e6",
    "email": "mina@example.com",
    "firstName": "Mina",
    "lastName": "Khan",
    "roles": ["Chef"],
    "createdAtUtc": "2026-08-16T12:32:05Z"
  }
}
```

Errors: `400 VALIDATION_ERROR` (bad input), `400 INVALID_ROLE` (role cannot be
self-assigned), `409 EMAIL_TAKEN` (email already registered),
`500 REGISTRATION_FAILED`.

Password rules: 8–128 characters, with at least one uppercase letter, one
lowercase letter, and one digit.

#### POST `/api/auth/login`

```json
// request
{ "email": "mina@example.com", "password": "Password123" }

// 200 response
{
  "data": {
    "id": "353c4c46-fc86-4b16-b2aa-fd8c46c573e6",
    "email": "mina@example.com",
    "firstName": "Mina",
    "lastName": "Khan",
    "roles": ["Chef"],
    "createdAtUtc": "2026-08-16T12:32:05Z"
  }
}
```

Errors: `401 INVALID_CREDENTIALS`, `423 LOCKED_OUT` (account lockout after
repeated failures), `400 VALIDATION_ERROR`.

#### POST `/api/auth/logout`

Requires authentication. Clears the cookie and returns `204 No Content`.

#### GET `/api/users/me`

Requires authentication. Returns the signed-in user.

```json
// 200 response
{
  "data": {
    "id": "353c4c46-fc86-4b16-b2aa-fd8c46c573e6",
    "email": "mina@example.com",
    "firstName": "Mina",
    "lastName": "Khan",
    "roles": ["Chef"],
    "createdAtUtc": "2026-08-16T12:32:05Z"
  }
}
```

Errors: `401` (missing or invalid credentials), `404 USER_NOT_FOUND`.

#### Error codes

| Code | Meaning |
| ---- | ------- |
| `VALIDATION_ERROR` | Model binding / validation failed |
| `INVALID_ROLE` | A role that cannot be self-assigned was requested |
| `EMAIL_TAKEN` | Email is already registered |
| `INVALID_CREDENTIALS` | Wrong email or password |
| `LOCKED_OUT` | Account is temporarily locked |
| `USER_NOT_FOUND` | No user matches the id |
| `REGISTRATION_FAILED` | Account creation failed unexpectedly |
| `NOT_FOUND` | Route/resource not found |
| `INTERNAL_ERROR` | Unhandled server error |

### Chef profiles (Stage 2)

| Method | Path | Description |
| ------ | ---- | ----------- |
| GET | `/api/chefs` | List public chef profiles (paginated, anonymous) |
| GET | `/api/chefs/{id}` | Public chef profile (anonymous) |
| GET | `/api/chefs/me` | Calling chef's own profile (Chef role) |
| POST | `/api/chefs/me` | Create calling chef's profile (Chef role) |
| PUT | `/api/chefs/me` | Update calling chef's profile (Chef role) |

#### GET `/api/chefs`

Public, paginated list ordered by display name.

```text
?page=1&pageSize=20
```

`page` defaults to 1; `pageSize` is clamped to 1–50 (default 20).

```json
// 200 response
{
  "data": [
    {
      "id": "17994471-e812-4da7-ae46-441555e5f09a",
      "displayName": "Amna's Kitchen",
      "bio": "Home-cooked Pakistani and continental dishes prepared fresh to order.",
      "city": "Karachi",
      "area": "Clifton",
      "cuisines": ["Bakery", "Pakistani"],
      "photoUrl": null
    }
  ],
  "meta": { "page": 1, "pageSize": 20, "total": 1, "hasMore": false }
}
```

#### GET `/api/chefs/{id}`

Returns one public profile; `404 CHEF_PROFILE_NOT_FOUND` if it does not exist.

#### GET `/api/chefs/me`

Returns the calling chef's profile; `404 CHEF_PROFILE_NOT_FOUND` when the chef
has not created one yet.

#### POST `/api/chefs/me`

Creates the calling chef's profile (first time). Returns `201` with the full
profile and sets no cookie. `409 CHEF_PROFILE_EXISTS` if a profile already
exists (use PUT to update).

```json
// request
{
  "displayName": "Amna's Kitchen",
  "bio": "Home-cooked Pakistani and continental dishes prepared fresh to order.",
  "city": "Karachi",
  "area": "Clifton",
  "cuisines": ["Pakistani", "Bakery"]
}
```

`cuisines` is optional; values are trimmed, de-duplicated case-insensitively,
and capped at 10 tags of 50 characters each.

#### PUT `/api/chefs/me`

Replaces the calling chef's profile. `404 CHEF_PROFILE_NOT_FOUND` when no
profile exists yet.

Errors: `401` (not signed in), `403` (not a Chef), `400 VALIDATION_ERROR`,
`404 CHEF_PROFILE_NOT_FOUND`, `409 CHEF_PROFILE_EXISTS`.

#### Error codes (chefs)

| Code | Meaning |
| ---- | ------- |
| `CHEF_PROFILE_NOT_FOUND` | No profile matches the id / user |
| `CHEF_PROFILE_EXISTS` | A profile already exists for this account |

## Pagination

List endpoints (added from Stage 2 onward) use `page` / `pageSize` query
parameters and return `meta` with `page`, `pageSize`, `total`, and `hasMore`.
No endpoint returns an unlimited collection.