# FCR Parser - Test Suite

This test project provides comprehensive unit test coverage for the FCR Parser application.

## Test Coverage

### Test Files

| Test File | Component Under Test | Test Count | Coverage Focus |
|-----------|---------------------|------------|----------------|
| `ColumnExtractorTests.cs` | `ColumnExtractor` | 16 | CSV column extraction, header detection, data cleaning |
| `TextCleanerTests.cs` | `TextCleaner` | 9 | Noise removal, whitespace handling, data sanitization |
| `JsonToTextConverterTests.cs` | `JsonToTextConverter` | 8 | JSON to text conversion, formatting |
| `ShipperExtractorTests.cs` | `ShipperExtractor` | 11 | AI provider fallback, JSON parsing, error handling |
| `DataExtractionOrchestratorTests.cs` | `DataExtractionOrchestrator` | 4 | Parallel execution, data combination |
| `AI/AIProviderTests.cs` | All AI Providers | 10 | API key validation, provider names, HTTP interactions |
| `Models/BookingDataTests.cs` | `BookingData` | 9 | Model properties, null handling, data integrity |

**Total Test Cases: 67+**

### Coverage Goals

- **Line Coverage**: 85%+
- **Branch Coverage**: 80%+
- **Method Coverage**: 90%+

## Running Tests

### Run All Tests

```bash
dotnet test
```

### Run Tests with Coverage

```bash
dotnet test --collect:"XPlat Code Coverage"
```

### Run Tests with Detailed Output

```bash
dotnet test --logger "console;verbosity=detailed"
```

### Run Specific Test Class

```bash
dotnet test --filter "FullyQualifiedName~ColumnExtractorTests"
```

### Run Specific Test Method

```bash
dotnet test --filter "FullyQualifiedName~ExtractMarksAndNumbers"
```

## Generate Coverage Report

### Install ReportGenerator (one-time)

```bash
dotnet tool install -g dotnet-reportgenerator-globaltool
```

### Generate HTML Coverage Report

```bash
# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Generate report (replace the path with actual coverage file)
reportgenerator -reports:"TestResults\*\coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html

# Open the report
start coveragereport\index.html
```

## Test Categories

### Unit Tests

All tests are unit tests that:
- Test individual components in isolation
- Use mocking for dependencies (via Moq)
- Execute quickly (< 100ms per test)
- Are deterministic and repeatable

### Test Patterns

#### Arrange-Act-Assert (AAA)

All tests follow the AAA pattern:

```csharp
[Fact]
public void TestMethod_ShouldDoSomething()
{
    // Arrange - Set up test data and dependencies
    var input = "test data";
    
    // Act - Execute the method under test
    var result = MethodUnderTest(input);
    
    // Assert - Verify the expected outcome
    Assert.Equal("expected", result);
}
```

#### Test Naming Convention

Tests follow the pattern: `MethodName_ShouldExpectedBehavior_WhenCondition`

Examples:
- `ExtractMarksAndNumbers_ShouldReturnData_WhenHeaderExists`
- `Clean_ShouldRemoveXMarkers_Always`
- `ExtractAsync_ShouldReturnNull_WhenAllProvidersFail`

## Key Testing Strategies

### 1. ColumnExtractor Tests

- **Header Detection**: Tests various header formats and positions
- **Data Extraction**: Validates correct column data extraction
- **Noise Filtering**: Ensures "x" markers and headers are removed
- **Edge Cases**: Empty files, missing headers, malformed CSV

### 2. TextCleaner Tests

- **Marker Removal**: Validates "x" marker removal
- **Comma Collapsing**: Tests multiple comma handling
- **Empty Line Removal**: Ensures whitespace-only lines are skipped
- **Data Preservation**: Verifies actual data is retained

### 3. ShipperExtractor Tests

- **Provider Fallback**: Tests sequential provider attempts
- **JSON Parsing**: Validates various JSON formats (with/without markdown)
- **Error Handling**: Tests invalid JSON, empty responses, exceptions
- **Case Insensitivity**: Ensures flexible JSON deserialization

### 4. AI Provider Tests

- **API Key Validation**: Tests behavior with missing keys
- **HTTP Mocking**: Uses Moq to simulate API responses
- **Error Scenarios**: Tests HTTP errors, timeouts, invalid responses

### 5. Integration Scenarios

- **DataExtractionOrchestrator**: Tests parallel execution of AI and column extraction
- **End-to-End**: Validates complete data flow from CSV to BookingData

## Mocking Strategy

### Dependencies Mocked

- `IAIProvider` - AI provider interface
- `HttpClient` - HTTP requests to external APIs
- `IConfiguration` - Configuration settings

### Example Mock Setup

```csharp
var mockProvider = new Mock<IAIProvider>();
mockProvider.Setup(p => p.Name).Returns("TestProvider");
mockProvider.Setup(p => p.GetResponseAsync(It.IsAny<string>()))
    .ReturnsAsync("{ \"ShipperName\": \"Test\" }");
```

## Test Data Management

### Temporary Files

Tests that require file I/O use temporary files:

```csharp
var tempFile = Path.GetTempFileName();
File.WriteAllText(tempFile, csvContent);

try
{
    // Test logic
}
finally
{
    File.Delete(tempFile); // Always cleanup
}
```

### Test Data Patterns

- **Minimal CSV**: Simple data for basic validation
- **Complex CSV**: Real-world FCR structure with "x" markers
- **Edge Cases**: Empty, null, malformed data

## Continuous Integration

### CI/CD Pipeline Integration

Add to your CI pipeline:

```yaml
- name: Run Tests
  run: dotnet test --configuration Release --collect:"XPlat Code Coverage"

- name: Generate Coverage Report
  run: reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coverage" -reporttypes:Html

- name: Fail if Coverage Below 85%
  run: |
    # Add coverage threshold check
```

## Troubleshooting

### Common Issues

| Issue | Solution |
|-------|----------|
| Tests fail with file access errors | Ensure temp files are properly cleaned up in `finally` blocks |
| Moq setup not working | Verify interface method signatures match exactly |
| Coverage report not generated | Ensure `coverlet.collector` package is installed |

## Contributing

When adding new features:

1. Write tests first (TDD approach)
2. Aim for 85%+ line coverage for new code
3. Follow existing test patterns
4. Update this README with new test categories

## Dependencies

- **xUnit** - Test framework
- **Moq** - Mocking library
- **coverlet.collector** - Code coverage collector
- **Microsoft.NET.Test.Sdk** - Test SDK

## References

- [xUnit Documentation](https://xunit.net/)
- [Moq Documentation](https://github.com/moq/moq4)
- [Code Coverage in .NET](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-code-coverage)
