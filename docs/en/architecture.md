# Architecture and Business Logic

## Architectural decisions

The project uses a layered structure:
- `Controllers` for HTTP/API contract and validation;
- `Services` for business logic;
- `Models/DTOs` for domain and transfer schemas;
- `Data/*.json` for file-based prototype storage.

## Business logic highlights

- Authentication flow in `AuthController` + `AuthService`.
- Car search flow in `CarsController.Search` + `CarService`.
- Reviews CRUD in `ReviewsController` + `ReviewService`.

## Component interaction

Client -> Controller -> Service -> Data -> Controller -> HTTP response.
