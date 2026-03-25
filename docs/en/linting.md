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

## 3. Instructions for Running the Linter

- `dotnet tool restore`
- `dotnet restore CarComparisonApi/CarComparisonApi.csproj`
- `dotnet restore CarComparisonApp.Tests/CarComparisonApp.Tests.csproj`
- `dotnet roslynator analyze CarComparisonApi/CarComparisonApi.csproj CarComparisonApp.Tests/CarComparisonApp.Tests.csproj`
