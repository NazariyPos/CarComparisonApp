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
- [Developer Onboarding (Fresh OS)](#developer-onboarding-fresh-os)
- [API Endpoints](#api-endpoints)
- [Project Structure](#project-structure)
- [Documentation Standards](#documentation-standards)
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

### Documentation Website (GitHub Pages)
The project documentation site is published with GitHub Pages from DocFX output.

Expected URL format:
- `https://<github-username>.github.io/CarComparisonApp/`

Repository setting required (one-time):
1. Open repository `Settings` -> `Pages`.
2. In `Build and deployment`, set `Source` to `GitHub Actions`.

### CI/CD for Documentation
A workflow is configured in `.github/workflows/docs-pages.yml`.

What it does on each push to `main`:
1. Checks out repository.
2. Installs .NET 8 SDK.
3. Installs DocFX.
4. Builds documentation via `docfx.json` with `--warningsAsErrors`.
5. Publishes `_site` to GitHub Pages.

Manual run is available via `workflow_dispatch` in GitHub Actions.

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

Automated run scripts:

Development (with hot reload):
```bash
./scripts/run-dev.sh
```
PowerShell:
```powershell
./scripts/run-dev.ps1
```

Production-like local run (Release publish + start):

> Before running, set `Jwt__Key` environment variable.

```bash
Jwt__Key='replace-with-secure-key' ./scripts/run-prod.sh
```
PowerShell:
```powershell
$Env:Jwt__Key = 'replace-with-secure-key'
./scripts/run-prod.ps1
```

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

### Documentation quality check
```bash
./scripts/verify-docs.sh
```
PowerShell:
```powershell
powershell -ExecutionPolicy Bypass -File scripts/verify-docs.ps1
```

Ignored by linting configuration:
- `**/bin/**`
- `**/obj/**`
- `**/*.g.cs`
- `**/*.g.i.cs`
- `**/*.Designer.cs`
- `CarComparisonApi/Data/**`

Detailed guides:
- English: `docs/en/linting.md`
- Українська: `docs/uk/linting.md`

## Developer Onboarding (Fresh OS)

A brief step-by-step guide for a developer with a fresh OS.

### 1) Install required software

Mandatory:
- `Git`
- `.NET SDK 8`

Optional (if you will run the React client):
- `Node.js` (LTS) + `npm`

Verify:
```bash
git --version
dotnet --version

### 2) Clone the repository

```bash
git clone https://github.com/vxmotors/CarComparisonApp.git
cd CarComparisonApp
```

### 3) Set up the development environment

Recommended:
- Visual Studio 2022 / Rider / VS Code (C# extension)

The project already includes:
- `Roslynator` (linting)
- `pre-commit` hook (`.githooks/pre-commit`)
- quality verification scripts (`scripts/verify.*`)

Enable git hooks:
```bash
git config core.hooksPath .githooks
```

### 4) Install and restore dependencies

```bash
dotnet tool restore
dotnet restore CarComparisonApi/CarComparisonApi.csproj
dotnet restore CarComparisonApp.Tests/CarComparisonApp.Tests.csproj
```

### 5) Database / storage

**No database is required (already implemented in the project):
- uses file‑based JSON storage in CarComparisonApi/Data/*.json.
- no separate database creation or migration step.

### 6) Configuration for running

Set JWT environment variables (PowerShell example):
```powershell
$Env:Jwt__Key = 'a-very-long-random-secret-32-chars-or-more'
$Env:Jwt__Issuer = 'CarComparisonApi'
$Env:Jwt__Audience = 'CarComparisonApiUsers'
```

### 7) Run in development mode

```bash
dotnet run --project CarComparisonApi/CarComparisonApi.csproj
```

After startup:
- API is available at the local URL shown in the console
- Swagger UI: `<base-url>/swagger`

### 8) Basic commands and operations

Build:
```bash
dotnet build CarComparisonApi/CarComparisonApi.csproj --no-restore
dotnet build CarComparisonApp.Tests/CarComparisonApp.Tests.csproj --no-restore
```

Tests:
```bash
dotnet test CarComparisonApp.Tests/CarComparisonApp.Tests.csproj
```

Linting:
```bash
dotnet roslynator analyze CarComparisonApi/CarComparisonApi.csproj CarComparisonApp.Tests/CarComparisonApp.Tests.csproj
```

Full verification:
```bash
./scripts/verify.sh
```
PowerShell:
```powershell
./scripts/verify.ps1
```

Documentation:
```bash
./scripts/verify-docs.sh
```
PowerShell:
```powershell
powershell -ExecutionPolicy Bypass -File scripts/verify-docs.ps1
```

## Documentation Standards

To ensure that all project participants document code in a uniform way, adhere to the following rules:

### 1) XML Documentation for Key APIs
Document `public` controllers and key methods using `///`:
- `<summary>` – brief description of what the class/method does;
- `<param>` – for each parameter;
- `<returns>` – what the method returns;
- `<remarks>` as needed for important constraints.

Minimum required documentation:
- new or modified endpoints in `Controllers/`;
- service methods that contain business logic.

### 2) Swagger (Swashbuckle) for HTTP API
For each endpoint, add:
- `SwaggerOperation` with a brief `Summary`;
- `ProducesResponseType` for the main response codes (`200/400/401/404/500` as needed).

This ensures up‑to‑date OpenAPI documentation in Swagger UI.

### 3) DocFX for Project Documentation
- Store English pages in `docs/en/` and Ukrainian pages in `docs/uk/`.
- The root `docs/index.md` acts as a language selector.
- Architectural decisions, business logic, difficult algorithms, and component interactions must be documented in both languages when relevant.
- When documentation rules change, update:
  - `docs/index.md` (if necessary),
  - `docs/toc.yml`,
  - language-specific files in `docs/en/` and `docs/uk/`.

### 4) What to Update for Each Feature
When adding or modifying functionality, check this checklist:
- [ ] XML comments for changed public methods are updated;
- [ ] Swagger attributes for endpoints are updated;
- [ ] `README.md` is updated if needed;
- [ ] a page in `docs/` is added or updated if needed.

### 5) Language and Style of Documentation
- For code and API attributes, use short, unambiguous wording in English.
- For internal guides in `docs/`, Ukrainian is allowed, but the style must be consistent within a single file.
- Be specific: what the method does, what input parameters are expected, what errors may occur.

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
