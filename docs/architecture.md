# Architecture, Business Logic, and Component Interaction

## 1. Architectural Decisions

The project follows a layered structure:

- `Controllers` — HTTP layer (input validation, status codes, API contract).
- `Services` — business logic and data handling.
- `Models` / `DTOs` — domain entities and data transfer objects.
- `Data/*.json` — file-based storage (prototype instead of a database).

Key decisions:
- Dependencies are injected via DI (`Program.cs`);
- Access to JSON files is encapsulated in services;
- Swagger (`Swashbuckle`) is used as the source of the API contract;
- DocFX is used to build project and API documentation.

## 2. Business Logic

### Authentication
- `AuthController` handles registration and login requests.
- `AuthService`:
  - checks for unique users;
  - hashes the password (`SHA256`);
  - generates a JWT token;
  - updates `LastLogin`.

### Car Search
- `CarsController.Search` validates filter combinations.
- `CarService.GetGenerationCardsAsync`:
  - filters the `Brand → Model → Generation → Trim` hierarchy;
  - discards empty results;
  - returns generation cards for the UI.

### Reviews
- `ReviewsController` manages CRUD operations on reviews.
- `ReviewService`:
  - validates the rating (`1..10`);
  - stores reviews in `reviews.json`;
  - builds enriched responses with user/car attributes.

## 3. Complex Algorithms

### Trim Comparison Algorithm (`ComparisonController`)

The `compare` endpoint accepts up to 4 `trimId` values and computes:
- "best" parameter value (`*_Best`)
- "worst" parameter value (`*_Worst`)

Special aspects:
- for parameters where lower is better (e.g., `Acceleration0To100`, `FuelConsumption`) inverted logic is used;
- ties are supported: if values are equal, all indices are preserved;
- when data is missing, fallback values (`0` or `decimal.MaxValue`) are applied for stable comparison.

## 4. Component Interaction

Typical request flow:

1. Client calls an HTTP endpoint in a `Controller`.
2. The controller performs validation and delegates to a `Service`.
3. The service reads/filters data (JSON or other services).
4. The service returns domain objects / DTOs.
5. The controller builds an HTTP response (`200/400/401/404/500`).

Example interaction for reviews:
- `ReviewsController` → `ReviewService`
- `ReviewService` → `AuthService` (user) + `CarService` (car hierarchy)
- aggregated data is returned as a single API response.

## 5. Documentation Maintenance Rule

When changes are made to logic/architecture, be sure to update:
- XML comments in code;
- Swagger attributes of endpoints;
- this file (`docs/architecture.md`) for architectural or algorithmic changes.
