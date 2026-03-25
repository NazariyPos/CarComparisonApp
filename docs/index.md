# Documentation

This documentation is available in two languages.

## Choose language

- English: `en/index.md`
- Українська: `uk/index.md`

## Build docs locally

```bash
dotnet tool restore
dotnet tool install docfx --tool-path ./.tools
./.tools/docfx docfx.json --serve
