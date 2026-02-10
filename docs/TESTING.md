# Testing Guide

## Test-Driven Development (TDD) Approach

This project follows Test-Driven Development principles. All features should be developed with tests first.

## Running Tests

### Prerequisites
- .NET 8.0 SDK installed
- Visual Studio or dotnet CLI

### Run All Tests
```powershell
dotnet test
```

### Run Tests with Coverage
```powershell
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

### Run Specific Test Project
```powershell
dotnet test tests/ClaudeCodeInstaller.Tests/ClaudeCodeInstaller.Tests.csproj
```

### Run Tests in Watch Mode
```powershell
dotnet watch test
```

## Test Structure

### Unit Tests
- Located in `tests/ClaudeCodeInstaller.Tests/`
- Test individual components in isolation
- Use mocking for external dependencies

### Integration Tests
- Test interactions between components
- May require actual system resources

### Test Categories
- **Fast**: Tests that run quickly (< 100ms)
- **Slow**: Tests that may take longer
- **System**: Tests that require system resources

## Writing Tests

### Example Test Structure
```csharp
[Fact]
public void MethodName_ShouldDoSomething_WhenCondition()
{
    // Arrange
    var service = new InstallationService();
    
    // Act
    var result = service.DoSomething();
    
    // Assert
    result.Should().Be(expected);
}
```

## Test Coverage Goals

- **Minimum**: 70% code coverage
- **Target**: 80% code coverage
- **Critical paths**: 100% coverage

## Continuous Integration

Tests run automatically on:
- Every commit
- Every pull request
- Before deployment

## Test Data

- Use test fixtures for common test data
- Clean up test data after tests
- Use isolated test environments
