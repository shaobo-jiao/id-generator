using Microsoft.Extensions.Options;

namespace IdGen.Api;

/// <summary>
/// Implements a Twitter Snowflake ID generator. 
/// </summary>
public class IdGenerator
{
    private readonly Lock _lock = new();
    private readonly TimeProvider _timeProvider; // time provider for ease of testing

    public IdStructure IdStructure { get; }
    public long WorkerId { get; }

    private Id? _lastId = null; // last generated ID;


    public IdGenerator(IOptions<IdOptions> idOptionsWrapper, TimeProvider timeProvider)
    {
        var idOptions = idOptionsWrapper.Value;
        IdStructure = new IdStructure(idOptions.Epoch, idOptions.TimestampBits, idOptions.WorkerIdBits, idOptions.SequenceBits);
        WorkerId = idOptions.WorkerId;

        _timeProvider = timeProvider;
    }

    /// <summary>
    /// Generates the next available ID.
    /// </summary>
    /// <returns></returns>
    public long NewId()
    {
        lock (_lock)
        {
            long timestamp = GetCurrentTimestamp();
            if (timestamp >= IdStructure.MaxTimestamp)
                throw new InvalidOperationException("Max Timestamp reached");

            // if dont have last id; sequence = 0, direct generate;
            if (_lastId is null)
            {
                // 1st id;
                _lastId = new Id(IdStructure, timestamp, WorkerId, 0);
                return _lastId.Value;
            }

            long sequence;
            if (timestamp < _lastId.Timestamp)
                throw new InvalidOperationException("Clock moved backwards");
            else if (timestamp == _lastId.Timestamp)
            {
                // same timestamp, increment sequence number
                if (_lastId.Sequence >= IdStructure.MaxSequence)
                {
                    // max sequence number reached, wait for next millisecond to refresh sequence number
                    timestamp = WaitUntilNextMillisecond(timestamp);
                    sequence = 0;
                }
                else
                    sequence = _lastId.Sequence + 1;
            }
            else
                sequence = 0;

            _lastId = new Id(IdStructure, timestamp, WorkerId, sequence);
            return _lastId.Value;
        }
    }

    private long GetCurrentTimestamp()
    {
        long now = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds();
        return now - IdStructure.Epoch;
    }

    private long WaitUntilNextMillisecond(long lastTimestamp)
    {
        long timestamp = GetCurrentTimestamp();
        while (timestamp <= lastTimestamp)
            timestamp = GetCurrentTimestamp();
        return timestamp;
    }
}
