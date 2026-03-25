# Documentation Generation

This guide describes how to generate project documentation using `Swashbuckle (Swagger)` and `DocFX`, and how it is published via GitHub Pages.

## Prerequisites

- Installed `.NET SDK 8+`
- Navigate to the repository root:

```bash
cd CarComparisonApp
```

## Generate OpenAPI JSON

```bash
dotnet run --project CarComparisonApi/CarComparisonApi.csproj --urls http://localhost:5056
curl http://localhost:5056/swagger/v1/swagger.json -o docs/swagger.json
```

## Generate DocFX site

```bash
dotnet tool install docfx --tool-path ./.tools
./.tools/docfx docfx.json
```

## Publish via GitHub Pages

Workflow: `.github/workflows/docs-pages.yml`
