# Skill: xUnit Testing — MGG Pulse

## Trigger
Load this skill when writing tests in MGG.Pulse.Tests.Unit.

---

## Test Project Conventions

- **Framework**: xUnit
- **Mocking**: Moq
- **Scope**: Domain + Application layers ONLY
- **No Win32**, no WinUI 3, no file system in unit tests

---

## Test Naming Convention

```
MethodName_StateUnderTest_ExpectedBehavior
```

```csharp
// Good
[Fact]
public void Evaluate_WhenUserIsIdle_ShouldReturnShouldExecuteTrue()

[Fact]
public async Task ExecuteAsync_WhenSimulationAlreadyRunning_ShouldReturnFailResult()

[Theory]
[InlineData(0)]
[InlineData(-1)]
public void IntervalRange_WhenMinIsNotPositive_ShouldThrowArgumentException(int min)
```

---

## Test Structure — AAA (Arrange / Act / Assert)

```csharp
public class IdleRuleTests
{
    [Fact]
    public void Evaluate_WhenIdleTimeExceedsThreshold_ShouldAllowExecution()
    {
        // Arrange
        var mockIdleDetector = new Mock<IIdleDetector>();
        mockIdleDetector.Setup(x => x.GetIdleTime()).Returns(TimeSpan.FromSeconds(60));

        var rule = new IdleRule(mockIdleDetector.Object, thresholdSeconds: 30);
        var context = new SimulationContext(SimulationMode.Intelligent);

        // Act
        var result = rule.Evaluate(context);

        // Assert
        Assert.True(result.ShouldExecute);
        Assert.Equal("Idle threshold exceeded", result.Reason);
    }
}
```

---

## Moq Patterns

```csharp
// Setup — return value
var mockRepo = new Mock<IConfigRepository>();
mockRepo.Setup(x => x.LoadAsync(It.IsAny<CancellationToken>()))
        .ReturnsAsync(new SimulationConfig(SimulationMode.Intelligent, new IntervalRange(30, 60), InputType.Mouse));

// Setup — throw exception
mockRepo.Setup(x => x.LoadAsync(It.IsAny<CancellationToken>()))
        .ThrowsAsync(new IOException("File not found"));

// Verify — method was called
mockRepo.Verify(x => x.SaveAsync(It.IsAny<SimulationConfig>(), It.IsAny<CancellationToken>()), Times.Once);

// Verify — method was never called
mockIdleDetector.Verify(x => x.GetIdleTime(), Times.Never);

// Strict mock — fails on unexpected calls (use for critical paths)
var strictMock = new Mock<IInputSimulator>(MockBehavior.Strict);
```

---

## Testing Rule Engine

```csharp
public class RuleEngineTests
{
    private readonly Mock<IIdleDetector> _mockIdleDetector = new();
    private readonly Mock<ILoggerService> _mockLogger = new();

    [Fact]
    public void Evaluate_AllRulesPass_ShouldExecute()
    {
        // Arrange
        _mockIdleDetector.Setup(x => x.GetIdleTime()).Returns(TimeSpan.FromMinutes(2));
        var engine = new RuleEngine(new IRule[]
        {
            new IdleRule(_mockIdleDetector.Object, thresholdSeconds: 30),
            new IntervalRule(lastExecutionTime: DateTime.UtcNow.AddMinutes(-2), minIntervalSeconds: 30)
        });

        var context = new SimulationContext(SimulationMode.Intelligent);

        // Act
        var result = engine.Evaluate(context);

        // Assert
        Assert.True(result.ShouldExecute);
    }

    [Fact]
    public void Evaluate_AggressiveMode_BypassesIdleRule()
    {
        // Arrange
        _mockIdleDetector.Setup(x => x.GetIdleTime()).Returns(TimeSpan.Zero); // user is active
        var engine = new RuleEngine(new IRule[]
        {
            new IdleRule(_mockIdleDetector.Object, thresholdSeconds: 30),
            new AggressiveModeRule()
        });

        var context = new SimulationContext(SimulationMode.Aggressive);

        // Act
        var result = engine.Evaluate(context);

        // Assert
        Assert.True(result.ShouldExecute);
        Assert.Contains("Aggressive", result.Reason);
    }
}
```

---

## Testing Use Cases

```csharp
public class StartSimulationUseCaseTests
{
    private readonly Mock<IRuleEngine> _mockEngine = new();
    private readonly Mock<IIdleDetector> _mockDetector = new();
    private readonly Mock<ILoggerService> _mockLogger = new();

    [Fact]
    public async Task ExecuteAsync_ValidConfig_ReturnsSuccessResult()
    {
        // Arrange
        var config = new SimulationConfig(SimulationMode.Intelligent, new IntervalRange(30, 60), InputType.Mouse);
        using var cts = new CancellationTokenSource();

        var useCase = new StartSimulationUseCase(_mockEngine.Object, _mockDetector.Object, _mockLogger.Object);

        // Act
        var result = await useCase.ExecuteAsync(config, cts.Token);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
    }

    [Fact]
    public async Task ExecuteAsync_CancellationRequested_ReturnsFailResult()
    {
        // Arrange
        var config = new SimulationConfig(SimulationMode.Intelligent, new IntervalRange(30, 60), InputType.Mouse);
        using var cts = new CancellationTokenSource();
        cts.Cancel(); // already cancelled

        var useCase = new StartSimulationUseCase(_mockEngine.Object, _mockDetector.Object, _mockLogger.Object);

        // Act
        var result = await useCase.ExecuteAsync(config, cts.Token);

        // Assert
        Assert.False(result.IsSuccess);
    }
}
```

---

## Testing Value Objects

```csharp
public class IntervalRangeTests
{
    [Fact]
    public void Constructor_ValidRange_CreatesInstance()
    {
        var range = new IntervalRange(30, 60);
        Assert.Equal(30, range.MinSeconds);
        Assert.Equal(60, range.MaxSeconds);
    }

    [Theory]
    [InlineData(0, 60)]
    [InlineData(-1, 60)]
    public void Constructor_MinIsNotPositive_ThrowsArgumentException(int min, int max)
    {
        Assert.Throws<ArgumentException>(() => new IntervalRange(min, max));
    }

    [Fact]
    public void Constructor_MaxLessThanMin_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new IntervalRange(60, 30));
    }

    [Fact]
    public void Equality_SameValues_AreEqual()
    {
        var a = new IntervalRange(30, 60);
        var b = new IntervalRange(30, 60);
        Assert.Equal(a, b); // record structural equality
    }
}
```

---

## File Organization

```
MGG.Pulse.Tests.Unit/
├── Domain/
│   ├── Entities/
│   │   ├── SimulationConfigTests.cs
│   │   └── SimulationSessionTests.cs
│   └── ValueObjects/
│       └── IntervalRangeTests.cs
└── Application/
    ├── Rules/
    │   ├── IdleRuleTests.cs
    │   ├── AggressiveModeRuleTests.cs
    │   ├── IntervalRuleTests.cs
    │   └── RuleEngineTests.cs
    └── UseCases/
        ├── StartSimulationUseCaseTests.cs
        └── StopSimulationUseCaseTests.cs
```

---

## Run Tests

```bash
dotnet test tests/MGG.Pulse.Tests.Unit

# With coverage
dotnet test tests/MGG.Pulse.Tests.Unit --collect:"XPlat Code Coverage"
```
