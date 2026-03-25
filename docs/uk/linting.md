# Лінтинг

У проєкті використовується `Roslynator`.

Базовий запуск:

```bash
dotnet tool restore
dotnet restore CarComparisonApi/CarComparisonApi.csproj
dotnet restore CarComparisonApp.Tests/CarComparisonApp.Tests.csproj
dotnet roslynator analyze CarComparisonApi/CarComparisonApi.csproj CarComparisonApp.Tests/CarComparisonApp.Tests.csproj
```
