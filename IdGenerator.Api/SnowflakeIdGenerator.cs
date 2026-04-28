using Microsoft.Extensions.Options;

namespace IdGenerator.Api;

/// <summary>
/// Implements a Twitter Snowflake ID generator. 
/// Epoch is defined as "2023-Jul-01 00:00:00". 
/// DataCenterId and MachineId are from configuration. 
/// </summary>
public class SnowflakeIdGenerator
{
    private readonly TimeProvider _timeProvider; // time provider for ease of testing
    private readonly Lock _lock = new();

    private const long _epoch = 1688169600000L; // "2023-Jul-01 00:00:00" 
    private readonly long _dataCenterId;
    private readonly long _machineId;

    private SnowflakeId? _lastId = null; // last generated ID;

    public SnowflakeIdGenerator(IOptions<SnowflakeIdGeneratorOptions> options, TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
        _dataCenterId = options.Value.DataCenterId;
        _machineId = options.Value.MachineId;
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
            if (timestamp >= SnowflakeId.MaxTimestamp)
                throw new InvalidOperationException("Max Timestamp reached");

            // if dont have last id; sequence = 0, direct generate;
            if (_lastId is null)
            {
                // 1st id;
                _lastId = new SnowflakeId(timestamp, _dataCenterId, _machineId, 0);
                return _lastId.Value;
            }

            long sequence;
            if (timestamp < _lastId.Timestamp)
                throw new InvalidOperationException("Clock moved backwards");
            else if (timestamp == _lastId.Timestamp)
            {
                // same timestamp, increment sequence number
                if (_lastId.Sequence >= SnowflakeId.MaxSequence)
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

            _lastId = new SnowflakeId(timestamp, _dataCenterId, _machineId, sequence);
            return _lastId.Value;
        }
    }

    private long GetCurrentTimestamp()
    {
        long now = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds();
        return now - _epoch;
    }

    private long WaitUntilNextMillisecond(long lastTimestamp)
    {
        long timestamp = GetCurrentTimestamp();
        while (timestamp <= lastTimestamp)
            timestamp = GetCurrentTimestamp();
        return timestamp;
    }
}
