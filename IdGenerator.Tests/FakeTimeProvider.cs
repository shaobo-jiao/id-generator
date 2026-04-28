namespace IdGenerator.Tests;

/// <summary>
/// Implements a fake time provider that ease testing of SnowflakeIdGenerator. 
/// </summary>
public class FakeTimeProvider : TimeProvider
{
    private DateTimeOffset _now;

    private int _readCount = 0; // tracks how many times GetUtcNow is called, useful for testing sequence number overflow

    /// <summary>
    /// max seq number is 4096. after reached, id generator loops until next millisecond.
    /// here we set a slightly higher threshold to simulate the behaviour that SnowflakeIdGenerator looping until next millisecond.
    /// </summary>
    private const int _maxReadsInOneMillisecond = 4100; 
    public FakeTimeProvider(DateTimeOffset now)
    {
        _now = now;
    }

    public override DateTimeOffset GetUtcNow()
    {
        _readCount++;
        if (_readCount >= _maxReadsInOneMillisecond)
        {
            AdvanceMilliseconds(1);
            _readCount = 0; // reset read count after advancing time
        }
        return _now;
    }

    public void AdvanceMilliseconds(long milliseconds)
    {
        _now = _now.AddMilliseconds(milliseconds);
    }
}
