# Linting in the Project

## 1) Selected Linter and Reasons for the Choice

The project uses `Roslynator`:
- The `Roslynator.Analyzers` package is added to the .NET projects;
- The `roslynator.dotnet.cli` CLI tool is added via the local `dotnet-tools.json` manifest.

Reasons for the choice:
- Native integration with the `C#/.NET` ecosystem and Roslyn;
- Stable operation with `net8.0`;
- Large number of rules for style, code simplification, and quality;
- Ability to run both in CI/CD and locally;
- Centralized rule configuration via `.editorconfig`.

## 2) Basic Rules and Their Explanation

The base configuration is placed in the `.editorconfig` file.

Key rule groups:
- Formatting:
  - `indent_style = space`, `indent_size = 4`;
  - `end_of_line = lf`, `insert_final_newline = true`, `trim_trailing_whitespace = true`.
- `using` directives:
  - `dotnet_sort_system_directives_first = true`;
  - `dotnet_separate_import_directive_groups = true`.
- C# style:
  - mandatory braces: `csharp_prefer_braces = true:warning`;
  - control of `var` usage (for more explicit code).
- Roslynator diagnostic severity:
  - `dotnet_analyzer_diagnostic.category-Roslynator.severity = warning`.

Ignoring (marking as generated code):
- `**/bin/**`
- `**/obj/**`
- `**/*.g.cs`
- `**/*.g.i.cs`
- `**/*.Designer.cs`
- `CarComparisonApi/Data/**`

## 3) Instructions for Running the Linter

### One‑time analysis run
```bash
dotnet tool restore
dotnet roslynator analyze CarComparisonApi/CarComparisonApi.csproj CarComparisonApp.Tests/CarComparisonApp.Tests.csproj
