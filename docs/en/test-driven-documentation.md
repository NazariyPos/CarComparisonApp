# Test-Driven Documentation

Examples are backed by executable unit tests.

Examples file:
- `CarComparisonApp.Tests/Documentation/ComponentUsageExamplesTests.cs`

Run only documentation examples:

```bash
dotnet test CarComparisonApp.Tests/CarComparisonApp.Tests.csproj --filter FullyQualifiedName~Documentation
```
