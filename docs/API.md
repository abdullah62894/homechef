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

## Pagination

List endpoints (added from Stage 2 onward) use `page` / `pageSize` query
parameters and return `meta` with `page`, `pageSize`, `total`, and `hasMore`.
No endpoint returns an unlimited collection.