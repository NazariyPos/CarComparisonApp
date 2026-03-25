# CarComparisonApi Documentation

This site is generated with `DocFX`.

## What is included

- API reference generated from XML comments and C# symbols.
- Architecture decisions and component interaction documentation.
- Business-logic documentation for core flows.
- Linting and documentation-generation guides.

## Main pages

- `architecture.md` — architecture decisions, business logic, complex algorithms, component interaction.
- `linting.md` — linting rules and quality gates.
- `generate_docs.md` — how to generate Swagger + DocFX documentation.

## Build docs locally

```bash
dotnet tool restore
dotnet tool install docfx --tool-path ./.tools
./.tools/docfx docfx.json --serve
