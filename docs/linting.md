# Linting

## 1. Chosen Linter and Reasons for the Choice

The project uses `Roslynator`:
- `Roslynator.Analyzers` is referenced in the `CarComparisonApi` and `CarComparisonApp.Tests` projects.
- `roslynator.dotnet.cli` is installed as a local tool via `dotnet-tools.json`.

Reasons for the choice:
- Native integration with `C#`/Roslyn;
- Works correctly with `.NET 8`;
- Checks style, quality, and code simplifications;
- Same execution locally and in CI/CD;
- Centralized configuration via `.editorconfig`.

## 2. Basic Rules and Their Explanation

The base configuration is located in `.editorconfig`.

Main rules:
- Formatting: `indent_style = space`, `indent_size = 4`, `end_of_line = lf`, `insert_final_newline = true`, `trim_trailing_whitespace = true`.
- `using` organisation: `dotnet_sort_system_directives_first = true`, `dotnet_separate_import_directive_groups = true`.
- C# style: `csharp_prefer_braces = true:warning`, rules for `var` usage.
- Roslynator rule severity: `dotnet_analyzer_diagnostic.category-Roslynator.severity = warning`.

Ignored paths/files:
- `**/bin/**`
- `**/obj/**`
- `**/*.g.cs`
- `**/*.g.i.cs`
- `**/*.Designer.cs`
- `CarComparisonApi/Data/**`

## 3. Instructions for Running the Linter

- Restore tools: `dotnet tool restore`
- Restore dependencies:
  - `dotnet restore CarComparisonApi/CarComparisonApi.csproj`
  - `dotnet restore CarComparisonApp.Tests/CarComparisonApp.Tests.csproj`
- Run the linter: `dotnet roslynator analyze CarComparisonApi/CarComparisonApi.csproj CarComparisonApp.Tests/CarComparisonApp.Tests.csproj`
- Run documentation quality checks:
  - `powershell -ExecutionPolicy Bypass -File scripts/verify-docs.ps1`
  - or `./scripts/verify-docs.sh`
- Check during build:
  - `dotnet build CarComparisonApi/CarComparisonApi.csproj --no-restore`
  - `dotnet build CarComparisonApp.Tests/CarComparisonApp.Tests.csproj --no-restore`

## Git hooks

A pre-commit hook is added: `.githooks/pre-commit`.

What the hook does before commit:
1. `dotnet tool restore`
2. `dotnet restore CarComparisonApi/CarComparisonApi.csproj`
3. `dotnet restore CarComparisonApp.Tests/CarComparisonApp.Tests.csproj`
4. `dotnet roslynator analyze ...`
5. `./scripts/verify-docs.sh`
6. `dotnet build CarComparisonApi/CarComparisonApi.csproj --no-restore`
7. `dotnet build CarComparisonApp.Tests/CarComparisonApp.Tests.csproj --no-restore`

Enabling the hooks:
- `./scripts/setup-git-hooks.ps1`
- or `./scripts/setup-git-hooks.sh`
- or manually: `git config core.hooksPath .githooks`

## Integration with the Build Process

Linting is integrated into the build process as follows:
- Roslynator analyzers are referenced in the projects, so they run during `dotnet build`;
- `EnforceCodeStyleInBuild = true` is enabled in `Directory.Build.props` to ensure style rules are checked during build.

## Comprehensive Code Verification

Added scripts:
- `scripts/verify.ps1`
- `scripts/verify.sh`
- `scripts/verify-docs.ps1`
- `scripts/verify-docs.sh`

Basic verification:
1. `dotnet tool restore`
2. `dotnet restore CarComparisonApi/CarComparisonApi.csproj`
3. `dotnet restore CarComparisonApp.Tests/CarComparisonApp.Tests.csproj`
4. `dotnet roslynator analyze ...`
5. documentation quality check via DocFX (`--warningsAsErrors`)
6. `dotnet build CarComparisonApi/CarComparisonApi.csproj --no-restore`
7. `dotnet build CarComparisonApp.Tests/CarComparisonApp.Tests.csproj --no-restore`

Optionally with tests:
- `./scripts/verify.ps1 -IncludeTests`
- `./scripts/verify.sh --include-tests`
