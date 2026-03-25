# Лінтинг

## 1. Обраний лінтер та причини вибору

У проєкті використовується `Roslynator`:
- `Roslynator.Analyzers` підключено до проєктів `CarComparisonApi` та `CarComparisonApp.Tests`.
- `roslynator.dotnet.cli` встановлено як локальний tool через `dotnet-tools.json`.

Причини вибору:
- Нативна інтеграція з `C#`/Roslyn;
- Коректна робота з `.NET 8`;
- Перевірка стилю, якості та спрощень коду;
- Однаковий запуск локально та в CI/CD;
- Централізована конфігурація через `.editorconfig`.

## 2. Базові правила та їх пояснення

Базова конфігурація знаходиться у `.editorconfig`.

Основні правила:
- Форматування: `indent_style = space`, `indent_size = 4`, `end_of_line = lf`, `insert_final_newline = true`, `trim_trailing_whitespace = true`.
- Організація `using`: `dotnet_sort_system_directives_first = true`, `dotnet_separate_import_directive_groups = true`.
- Стиль C#: `csharp_prefer_braces = true:warning`, правила використання `var`.
- Рівень правил Roslynator: `dotnet_analyzer_diagnostic.category-Roslynator.severity = warning`.

Ігноровані шляхи/файли:
- `**/bin/**`
- `**/obj/**`
- `**/*.g.cs`
- `**/*.g.i.cs`
- `**/*.Designer.cs`
- `CarComparisonApi/Data/**`

## 3. Інструкція з запуску лінтера

- Відновлення інструментів: `dotnet tool restore`
- Відновлення залежностей:
  - `dotnet restore CarComparisonApi/CarComparisonApi.csproj`
  - `dotnet restore CarComparisonApp.Tests/CarComparisonApp.Tests.csproj`
- Запуск лінтера: `dotnet roslynator analyze CarComparisonApi/CarComparisonApi.csproj CarComparisonApp.Tests/CarComparisonApp.Tests.csproj`
- Запуск перевірки документації:
  - `powershell -ExecutionPolicy Bypass -File scripts/verify-docs.ps1`
  - або `./scripts/verify-docs.sh`
- Перевірка під час збірки:
  - `dotnet build CarComparisonApi/CarComparisonApi.csproj --no-restore`
  - `dotnet build CarComparisonApp.Tests/CarComparisonApp.Tests.csproj --no-restore`

## Git hooks

Додано pre-commit hook: `.githooks/pre-commit`.

Що виконує хук перед комітом:
1. `dotnet tool restore`
2. `dotnet restore CarComparisonApi/CarComparisonApi.csproj`
3. `dotnet restore CarComparisonApp.Tests/CarComparisonApp.Tests.csproj`
4. `dotnet roslynator analyze ...`
5. `./scripts/verify-docs.sh`
6. `dotnet build CarComparisonApi/CarComparisonApi.csproj --no-restore`
7. `dotnet build CarComparisonApp.Tests/CarComparisonApp.Tests.csproj --no-restore`

Увімкнення хуків:
- `./scripts/setup-git-hooks.ps1`
- або `./scripts/setup-git-hooks.sh`
- або вручну: `git config core.hooksPath .githooks`

## Інтеграція з процесом збірки

Лінтинг інтегровано у процес збірки так:
- аналізатори Roslynator підключені у проєктах, тому працюють під час `dotnet build`;
- у `Directory.Build.props` увімкнено `EnforceCodeStyleInBuild = true`, щоб стильові правила перевірялись під час збірки.

## Комплексна перевірка коду

Додані скрипти:
- `scripts/verify.ps1`
- `scripts/verify.sh`
- `scripts/verify-docs.ps1`
- `scripts/verify-docs.sh`

Базова перевірка:
1. `dotnet tool restore`
2. `dotnet restore CarComparisonApi/CarComparisonApi.csproj`
3. `dotnet restore CarComparisonApp.Tests/CarComparisonApp.Tests.csproj`
4. `dotnet roslynator analyze ...`
5. перевірка якості документації через DocFX (`--warningsAsErrors`)
6. `dotnet build CarComparisonApi/CarComparisonApi.csproj --no-restore`
7. `dotnet build CarComparisonApp.Tests/CarComparisonApp.Tests.csproj --no-restore`

Опційно з тестами:
- `./scripts/verify.ps1 -IncludeTests`
- `./scripts/verify.sh --include-tests`
