using MGG.Pulse.Domain.ValueObjects;
using Xunit;

namespace MGG.Pulse.Tests.Unit.Domain.ValueObjects;

public class IntervalRangeTests
{
    [Fact]
    public void Constructor_ValidRange_CreatesInstance()
    {
        var range = new IntervalRange(30, 60);

        Assert.Equal(30, range.MinSeconds);
        Assert.Equal(60, range.MaxSeconds);
    }

    [Fact]
    public void Constructor_MinEqualsMax_IsFixed()
    {
        var range = new IntervalRange(30, 30);

        Assert.True(range.IsFixed);
        Assert.Equal(30, range.MinSeconds);
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

        Assert.Equal(a, b);
    }

    [Fact]
    public void Equality_DifferentValues_AreNotEqual()
    {
        var a = new IntervalRange(30, 60);
        var b = new IntervalRange(30, 90);

        Assert.NotEqual(a, b);
    }

    [Fact]
    public void IsFixed_DifferentMinMax_ReturnsFalse()
    {
        var range = new IntervalRange(30, 60);

        Assert.False(range.IsFixed);
    }
}
