using Microsoft.Extensions.Options;

namespace IdGenerator.Api;

/// <summary>
/// Implements a Twitter Snowflake like long-integer ID generator. 
/// 
/// The bits of an ID are partitioned as follows:
/// - bits[0]: sign bit, always 0
/// - bits[1..41]: timestamp bits representing milliseconds since a custom epoch, 41 bits
/// - bits[42..46]: datacenter ID, 5 bits
/// - bits[47..51]: machine ID, 5 bits
/// - bits[52..63]: sequence number, 12 bits
/// </summary>
public class IdGenerator
{
    private readonly Lock _lock = new();

    #region constants
    private const long _epoch = 1688169600000L; // "2023-Jul-01 00:00:00" 

    private const int _timestampBits = 41;
    private const int _dataCenterIdBits = 5;
    private const int _machineIdBits = 5;
    private const int _sequenceBits = 12;

    private const long _maxTimestamp = (1L << _timestampBits) - 1;
    public int MaxDataCenterId => (1 << _dataCenterIdBits) - 1; // make public since it's specified outside
    public int MaxMachineId => (1 << _machineIdBits) - 1; // make public since it's specified outside
    private const int _maxSequence = (1 << _sequenceBits) - 1;
    #endregion

    private readonly long _dataCenterId;
    private readonly long _machineId;
    private long _lastSequence = -1;
    private long _lastTimestamp = -1;

    private readonly TimeProvider _timeProvider; // time provider for ease of
    public IdGenerator(IOptions<IdGeneratorOptions> options, TimeProvider timeProvider)
    {
        // the options object is considered valid.
        _dataCenterId = options.Value.DataCenterId;
        _machineId = options.Value.MachineId;
        _timeProvider = timeProvider;
    }

    /// <summary>
    /// Generates next ID.
    /// </summary>
    /// <returns></returns>
    public long NewId()
    {
        lock (_lock)
        {
            long timestamp = GetCurrentTimestamp();

            if (timestamp >= _maxTimestamp)
                throw new InvalidOperationException("Max Timestamp reached");

            if (timestamp < _lastTimestamp)
                throw new InvalidOperationException("Clock moved backwards");

            long sequence;
            if (timestamp == _lastTimestamp)
            {
                // same timestamp, increment sequence number
                if (_lastSequence >= _maxSequence)
                {
                    // max sequence number reached, wait for next millisecond to refresh sequence number
                    timestamp = WaitNextMillisecond(timestamp);
                    sequence = 0;
                }
                else
                    sequence = _lastSequence + 1;
            }
            else
                sequence = 0; // reset sequence number for a new timestamp;

            _lastTimestamp = timestamp;
            _lastSequence = sequence;
            return BuildId(timestamp, _dataCenterId, _machineId, sequence);
        }
    }

    private long GetCurrentTimestamp()
    {
        long now = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds();
        return now - _epoch;
    }

    private long WaitNextMillisecond(long lastTimestamp)
    {
        long timestamp = GetCurrentTimestamp();
        while (timestamp <= lastTimestamp)
            timestamp = GetCurrentTimestamp();
        return timestamp;
    }

    private static long BuildId(long timestamp, long dataCenterId, long machineId, long sequence)
    {
        return (timestamp << (_dataCenterIdBits + _machineIdBits + _sequenceBits))
            | (dataCenterId << (_machineIdBits + _sequenceBits))
            | (machineId << _sequenceBits)
            | sequence;
    }
}
