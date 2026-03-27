# Logging Strategy (Serilog)

## 1) Error types in the project

For `CarComparisonApi`, the main error groups are:

1. **Request validation errors**
   - invalid search parameters;
   - invalid review rating;
   - invalid login/password format.

2. **Authentication/authorization errors**
   - invalid credentials;
   - missing/expired JWT;
   - insufficient access rights.

3. **Data access errors**
   - missing `Data/*.json` file;
   - JSON read/write failure;
   - invalid/corrupted JSON format.

4. **Infrastructure/runtime errors**
   - unexpected exceptions during startup/shutdown;
   - middleware/pipeline failures;
   - unavailable or misconfigured dependencies.

## 2) Logging levels

Implemented base levels:

- `DEBUG` — technical details (file save operations, helper execution details).
- `INFO` — normal business events (successful login/registration, create/update/delete operations).
- `WARNING` — suspicious or expected-problem scenarios (failed login, update for non-existing record).
- `ERROR` — operation-level failures (data read/write errors, service exceptions).
- `CRITICAL` (`Fatal` in Serilog) — application crash or unrecoverable failure.

## 3) Serilog integration

Implemented:

1. Added packages:
   - `Serilog.AspNetCore`
   - `Serilog.Settings.Configuration`
   - `Serilog.Sinks.Console`
   - `Serilog.Sinks.File`

2. Configured levels and overrides in `appsettings.json`:
   - `Default=Information`
   - `CarComparisonApi=Debug`
   - `Microsoft/Microsoft.AspNetCore/System=Warning`

3. Configured base log format:
   - `Timestamp`
   - `Level`
   - `SourceContext` (module)
   - `Message`
   - `Exception`

4. Configured outputs:
   - console
   - file `logs/carcomparison-.log` (daily rolling)

## 4) Where base logging was added

1. **Application startup and shutdown** (`Program.cs`):
   - host startup;
   - `ApplicationStarted`, `ApplicationStopping`, `ApplicationStopped`;
   - `Fatal` on unexpected termination.

2. **Critical data operations**:
   - `JsonUserService`: load/save/create/update/delete users;
   - `ReviewService`: load/save/CRUD reviews;
   - `AuthService`: registration, login, and user access paths.

3. **Errors and exceptions**:
   - file read/write failures (`Error`);
   - service exceptions (`Error`);
   - fatal lifecycle errors (`Fatal`).
