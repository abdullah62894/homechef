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
| `CHEF_PROFILE_REQUIRED` | A chef profile must be created before adding menu items |

### Food and Menus (Stage 3)

| Method | Path | Description |
| ------ | ---- | ----------- |
| GET | `/api/foods` | List public food items (paginated, filter by categoryId, chefId, search, isAvailable) |
| GET | `/api/foods/{id}` | Get single food item details (anonymous) |
| GET | `/api/foods/categories` | List standard food categories (anonymous) |
| GET | `/api/chefs/{chefId}/foods` | List public food items for a specific chef (anonymous, paginated) |
| GET | `/api/chefs/me/foods` | List caller's own food items (Chef role, paginated) |
| POST | `/api/chefs/me/foods` | Create new food item (Chef role) |
| PUT | `/api/chefs/me/foods/{id}` | Update food item (Chef role, verifies ownership) |
| DELETE | `/api/chefs/me/foods/{id}` | Delete food item (Chef role, verifies ownership) |
| PATCH | `/api/chefs/me/foods/{id}/availability` | Toggle availability status (Chef role, verifies ownership) |

#### GET `/api/foods`

Public, paginated query filter:

```text
?categoryId={guid}&chefId={guid}&search={string}&isAvailable={bool}&page=1&pageSize=20
```

```json
// 200 response
{
  "data": [
    {
      "id": "44444444-4444-4444-4444-444444444444",
      "chefProfileId": "17994471-e812-4da7-ae46-441555e5f09a",
      "chefDisplayName": "Amna's Kitchen",
      "chefCity": "Karachi",
      "chefArea": "Clifton",
      "categoryId": "11111111-1111-1111-1111-111111111102",
      "categoryName": "Rice & Biryani",
      "name": "Special Chicken Biryani",
      "description": "Fragrant basmati rice layered with spiced marinated chicken and potatoes.",
      "price": 650.00,
      "currency": "PKR",
      "isAvailable": true,
      "imageUrl": null,
      "preparationTimeMinutes": 45
    }
  ],
  "meta": { "page": 1, "pageSize": 20, "total": 1, "hasMore": false }
}
```

#### POST `/api/chefs/me/foods`

```json
// request
{
  "name": "Special Chicken Biryani",
  "description": "Fragrant basmati rice layered with spiced marinated chicken and potatoes.",
  "price": 650.00,
  "currency": "PKR",
  "categoryId": "11111111-1111-1111-1111-111111111102",
  "isAvailable": true,
  "preparationTimeMinutes": 45
}
```

#### Error codes (food)

| Code | Meaning |
| ---- | ------- |
| `FOOD_ITEM_NOT_FOUND` | No food item matches the id |
| `FOOD_CATEGORY_NOT_FOUND` | Specified category does not exist |
| `FOOD_ITEM_FORBIDDEN` | Caller does not own the requested food item |

### Search and Locations (Stage 4)

| Method | Path | Description |
| ------ | ---- | ----------- |
| GET | `/api/search` | Search chefs and food items (query, location, cuisine, coordinates, radius, type) |
| GET | `/api/locations` | Location directory: all cities with areas and chef counts |
| GET | `/api/locations/{city}` | Specific city location summary and areas |
| GET | `/api/locations/{city}/{area}` | Chefs in a specific city and area |

#### GET `/api/search`

Public query filter:

```text
?q={string}&city={string}&area={string}&cuisine={string}&categoryId={guid}&lat={double}&lng={double}&radiusKm={double}&type={all|chefs|foods}&page=1&pageSize=20
```

```json
// 200 response
{
  "data": {
    "chefs": [
      {
        "id": "17994471-e812-4da7-ae46-441555e5f09a",
        "displayName": "Amna's Kitchen",
        "bio": "Home-cooked Pakistani dishes.",
        "city": "Karachi",
        "area": "Clifton",
        "address": "Block 2, Clifton",
        "latitude": 24.8138,
        "longitude": 67.0298,
        "distanceKm": 1.25,
        "cuisines": ["Pakistani"],
        "photoUrl": null
      }
    ],
    "foods": [
      {
        "id": "44444444-4444-4444-4444-444444444444",
        "chefProfileId": "17994471-e812-4da7-ae46-441555e5f09a",
        "chefDisplayName": "Amna's Kitchen",
        "chefCity": "Karachi",
        "chefArea": "Clifton",
        "chefAddress": "Block 2, Clifton",
        "distanceKm": 1.25,
        "categoryId": "11111111-1111-1111-1111-111111111102",
        "categoryName": "Rice & Biryani",
        "name": "Special Chicken Biryani",
        "description": "Fragrant basmati rice layered with spiced marinated chicken.",
        "price": 650.00,
        "currency": "PKR",
        "isAvailable": true,
        "imageUrl": null,
        "preparationTimeMinutes": 45
      }
    ],
    "totalChefs": 1,
    "totalFoods": 1,
    "page": 1,
    "pageSize": 20
  }
}
```

#### GET `/api/locations`

```json
// 200 response
{
  "data": {
    "cities": [
      {
        "city": "Karachi",
        "totalChefs": 12,
        "areas": [
          { "name": "Clifton", "chefCount": 7 },
          { "name": "DHA", "chefCount": 5 }
        ]
      }
    ]
  }
}
```

### Reviews and Ratings (Stage 5)

| Method | Path | Description |
| ------ | ---- | ----------- |
| GET | `/api/chefs/{chefId}/reviews` | Public paginated list of reviews for a chef |
| GET | `/api/chefs/{chefId}/reviews/summary` | Rating average, total reviews count, and 1–5 star distribution |
| POST | `/api/chefs/{chefId}/reviews` | Submit a review and 1–5 star rating (Customer role / authenticated user) |
| PUT | `/api/reviews/{id}` | Update rating and comment (Review owner only) |
| DELETE | `/api/reviews/{id}` | Delete review (Review owner only) |

#### POST `/api/chefs/{chefId}/reviews`

```json
// request
{
  "rating": 5,
  "comment": "Outstanding homemade taste and delivered fresh!"
}
```

```json
// 201 response
{
  "data": {
    "id": "68379294-8149-43c2-bf77-1f4806a6b579",
    "chefProfileId": "17994471-e812-4da7-ae46-441555e5f09a",
    "customerUserId": "cce11a51-de1d-4cf7-b4d2-f48ea6a957f2",
    "customerName": "Sara Ali",
    "rating": 5,
    "comment": "Outstanding homemade taste and delivered fresh!",
    "createdAtUtc": "2026-08-16T14:10:00Z",
    "updatedAtUtc": "2026-08-16T14:10:00Z"
  }
}
```

#### GET `/api/chefs/{chefId}/reviews/summary`

```json
// 200 response
{
  "data": {
    "chefProfileId": "17994471-e812-4da7-ae46-441555e5f09a",
    "averageRating": 4.8,
    "totalReviews": 15,
    "ratingDistribution": {
      "1": 0,
      "2": 0,
      "3": 1,
      "4": 3,
      "5": 11
    }
  }
}
```

#### Error codes (reviews)

| Code | Meaning |
| ---- | ------- |
| `REVIEW_NOT_FOUND` | No review matches the id |
| `SELF_REVIEW_FORBIDDEN` | Chefs cannot review their own kitchen |
| `DUPLICATE_REVIEW` | Customer has already reviewed this chef |
| `REVIEW_FORBIDDEN` | Caller does not own the requested review |

### Favorites (Stage 6)

| Method | Path | Description |
| ------ | ---- | ----------- |
| POST | `/api/favorites/chefs/{chefId}` | Add chef to user's favorites (Authorize) |
| DELETE | `/api/favorites/chefs/{chefId}` | Remove chef from user's favorites (Authorize) |
| GET | `/api/favorites/chefs` | List authenticated user's favorite chefs (Authorize, paginated) |
| POST | `/api/favorites/foods/{foodId}` | Add food item to user's favorites (Authorize) |
| DELETE | `/api/favorites/foods/{foodId}` | Remove food item from user's favorites (Authorize) |
| GET | `/api/favorites/foods` | List authenticated user's favorite foods (Authorize, paginated) |
| GET | `/api/favorites/ids` | Get complete set of favorited chef and food IDs for user (Authorize) |

#### GET `/api/favorites/ids`

```json
// 200 response
{
  "data": {
    "chefIds": [
      "17994471-e812-4da7-ae46-441555e5f09a"
    ],
    "foodIds": [
      "44444444-4444-4444-4444-444444444444"
    ]
  }
}
```

## Pagination

List endpoints use `page` / `pageSize` query parameters and return `meta` with `page`, `pageSize`, `total`, and `hasMore`.
No endpoint returns an unlimited collection.