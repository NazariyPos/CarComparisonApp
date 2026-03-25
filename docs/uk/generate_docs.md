# Генерація документації

Swagger JSON:

```bash
dotnet run --project CarComparisonApi/CarComparisonApi.csproj --urls http://localhost:5056
curl http://localhost:5056/swagger/v1/swagger.json -o docs/swagger.json
```

DocFX:

```bash
dotnet tool install docfx --tool-path ./.tools
./.tools/docfx docfx.json
```
