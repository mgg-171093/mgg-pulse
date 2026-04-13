namespace MGG.Pulse.Domain.ValueObjects;

public sealed record IntervalRange
{
    public int MinSeconds { get; }
    public int MaxSeconds { get; }

    public IntervalRange(int minSeconds, int maxSeconds)
    {
        if (minSeconds <= 0)
            throw new ArgumentException("MinSeconds must be greater than 0.", nameof(minSeconds));
        if (maxSeconds < minSeconds)
            throw new ArgumentException("MaxSeconds must be greater than or equal to MinSeconds.", nameof(maxSeconds));

        MinSeconds = minSeconds;
        MaxSeconds = maxSeconds;
    }

    public bool IsFixed => MinSeconds == MaxSeconds;
}
