# CarComparisonApp

**CarComparisonApp** is a web application for comparing cars based on various specifications. 

## Table of Contents
- [Description](#description)
- [Features](#features)
- [Tech Stack](#tech-stack)
- [Installation and Setup](#installation-and-setup)
  - [Prerequisites](#prerequisites)
  - [Cloning the Repository](#cloning-the-repository)
  - [Configuration](#configuration)
  - [Running the Application](#running-the-application)
- [API Endpoints](#api-endpoints)
- [Project Structure](#project-structure)
- [Contributing](#contributing)
- [License](#license)
- [Author](#author)

---

## Description

CarComparisonApp allows users to browse a catalog of cars, add them to a comparison list, and analyze their technical specifications. An admin panel provides the ability to manage cars and users. Authentication is handled using JWT tokens, ensuring secure access to the API.

## Features

- Register / Login with JWT tokens
- Get current user information
- List/search brands, models, generations and trims
- Generation cards and full trim details
- Compare trims by specifications
- Create/update/delete reviews
- Favorites per user
- Example React frontend (optional)

## Tech Stack

- Backend: ASP.NET Core 8 Web API
- Persistence (prototype): JSON files (`CarComparisonApi/Data/*.json`)
- Authentication: JWT (symmetric keys)
- API docs: Swagger
- Frontend: example React + Vite (in `carcomparisonclient/`)
- Testing: xUnit (in `CarComparisonApp.Tests/`)

## Installation and Setup

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Node.js + npm (optional, for the React client)
- Git

### Cloning the Repository
```bash
git clone https://github.com/vxmotors/CarComparisonApp.git 
cd CarComparisonApp
```

### Configuration
Required values:
- `Jwt:Key` (environment variable: `Jwt__Key`) — symmetric signing key (strong random string, >= 32 chars)
- `Jwt:Issuer` (env: `Jwt__Issuer`)
- `Jwt:Audience` (env: `Jwt__Audience`)

Optional:
- `Jwt:ExpireDays` (env: `Jwt__ExpireDays`) — token lifetime in days (default is 7)

Set environment variables (examples):

PowerShell (current session)
```powershell
$Env:Jwt__Key = 'a-very-long-random-secret-32-chars-or-more' 
$Env:Jwt__Issuer = 'CarComparisonApi' 
$Env:Jwt__Audience = 'CarComparisonApiUsers'
```
### Running the Application
```bash
dotnet run
```
After startup, the API will be available on configured HTTP/HTTPS ports.

### Linting (Roslynator)
Roslynator analyzers are connected to both projects and configured via `.editorconfig`.

Run linting:
```bash
dotnet tool restore
dotnet restore CarComparisonApi/CarComparisonApi.csproj
dotnet restore CarComparisonApp.Tests/CarComparisonApp.Tests.csproj
dotnet roslynator analyze CarComparisonApi/CarComparisonApi.csproj CarComparisonApp.Tests/CarComparisonApp.Tests.csproj
```

### Git hooks
Pre-commit hook is available in `.githooks/pre-commit` and runs lint + build checks before commit.

Enable hooks:
```bash
git config core.hooksPath .githooks
```

### Integration with the build process
Linting is integrated into build because:
- analyzers run during build;
- `Directory.Build.props` enables `EnforceCodeStyleInBuild`.

Build commands:
```bash
dotnet build CarComparisonApi/CarComparisonApi.csproj --no-restore
dotnet build CarComparisonApp.Tests/CarComparisonApp.Tests.csproj --no-restore
```

### Full code quality check
```bash
./scripts/verify.sh
```
PowerShell:
```powershell
./scripts/verify.ps1
```

Ignored by linting configuration:
- `**/bin/**`
- `**/obj/**`
- `**/*.g.cs`
- `**/*.g.i.cs`
- `**/*.Designer.cs`
- `CarComparisonApi/Data/**`

Detailed guide: `docs/linting.md`

## API Endpoints

Authentication
- POST /api/auth/register  
  Body: RegisterRequest { Login, Email, Password, RealName? }  
  Returns: AuthResponse { Token, User }

- POST /api/auth/login  
  Body: LoginRequest { LoginOrEmail, Password }  
  Returns: AuthResponse { Token, User }

- GET /api/auth/me  
  Header: Authorization: Bearer <token>  
  Returns: current user information

Cars (examples)
- GET /api/cars/search?brand=Toyota&model=Camry — search/generation cards
- GET /api/cars/generation/{id} — generation with trims
- GET /api/cars/trim/{id} — full trim details
- POST /api/comparison — compare multiple trim IDs (see controller for exact shape)

Reviews
- GET /api/reviews/trim/{trimId}
- POST /api/reviews (Authorized) — create review
- PUT /api/reviews/{id} (Authorized owner) — update review
- DELETE /api/reviews/{id} (Authorized owner) — delete review

## Project Structure

- CarComparisonApi/
  - Controllers/
  - Services/
  - Models/
  - Data/
  - Program.cs
  - appsettings.json.example

- carcomparisonclient/
- CarComparisonApp.Tests/

## Contributing

- Fork the repo and create a feature branch.
- Run tests and add unit tests for new features.
- Do not commit secrets or personal data.
- Open a Pull Request with a clear description of changes.

## License

This project is provided under the MIT License. See `LICENSE` for details.

---

## Author

Nazarii Pos
