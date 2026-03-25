# Test-Driven Documentation ("Living" Documentation)

This page describes an approach where tests serve as examples of component usage.

## Idea Behind the Approach

Instead of static examples in documentation, actual unit tests are used:

- an example = a test scenario;
- the validity of the example is confirmed by running the tests;
- when the API / logic changes, the tests fail, so the documentation does not become silently outdated.

## Where the Examples Are Located

The examples file is:

- `CarComparisonApp.Tests/Documentation/ComponentUsageExamplesTests.cs`

It contains working scenarios:

1. `Example_Register_User_With_AuthController`
   - an example of registration via `AuthController`.
2. `Example_Search_Generations_With_CarsController`
   - an example of searching generations via `CarsController.Search`.
3. `Example_Compare_Trims_With_ComparisonController`
   - an example of comparing trims via `ComparisonController.Compare`.

## How to Run the "Living" Documentation

Run only the documentation examples:

```bash
dotnet test CarComparisonApp.Tests/CarComparisonApp.Tests.csproj --filter FullyQualifiedName~Documentation
```

Or run the full project verification:

```bash
./scripts/verify.sh --include-tests
```

PowerShell:

```powershell
powershell -ExecutionPolicy Bypass -File scripts/verify.ps1 -IncludeTests
```

## Rule for New Examples

When a new public component or an important new scenario is added:

1. Add/update the XML + Swagger documentation in the code.
2. Add a test example in CarComparisonApp.Tests/Documentation.
3. Add a brief description of the scenario on this page.
