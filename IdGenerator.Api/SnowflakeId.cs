namespace IdGenerator.Api;

/// <summary>
/// represents a SnowflakeId generated from SnowflakeIdGenerator
/// 
/// The bits of an ID are partitioned as follows:
/// - bits[0]: sign bit, always 0
/// - bits[1..41]: timestamp bits representing milliseconds since a custom epoch, 41 bits
/// - bits[42..46]: datacenter ID, 5 bits
/// - bits[47..51]: machine ID, 5 bits
/// - bits[52..63]: sequence number, 12 bits
/// </summary>
public class SnowflakeId
{
    #region SnowflakeId partition constants    
    private const int _timestampBits = 41;
    private const int _dataCenterIdBits = 5;
    private const int _machineIdBits = 5;
    private const int _sequenceBits = 12;

    public const long MaxTimestamp = (1L << _timestampBits) - 1;
    public const int MaxDataCenterId = (1 << _dataCenterIdBits) - 1; // make public since it's specified outside
    public const int MaxMachineId = (1 << _machineIdBits) - 1; // make public since it's specified outside
    public const int MaxSequence = (1 << _sequenceBits) - 1;
    #endregion

    public long Value { get; }
    public long Timestamp { get; }
    public long DataCenterId { get; }
    public long MachineId { get; }
    public long Sequence { get; }

    public SnowflakeId(long value)
    {
        Value = value;
        (Timestamp, DataCenterId, MachineId, Sequence) = DecomposeId(value);
    }

    public SnowflakeId(long timestamp, long dataCenterId, long machineId, long sequence)
    {
        Timestamp = timestamp;
        DataCenterId = dataCenterId;
        MachineId = machineId;
        Sequence = sequence;
        Value = ComposeId(timestamp, dataCenterId, machineId, sequence);
    }

    private static long ComposeId(long timestamp, long dataCenterId, long machineId, long sequence)
    {
        return (timestamp << (_dataCenterIdBits + _machineIdBits + _sequenceBits))
            | (dataCenterId << (_machineIdBits + _sequenceBits))
            | (machineId << _sequenceBits)
            | sequence;
    }

    private static (long timestamp, long dataCenterId, long machineId, long sequence) DecomposeId(long id)
    {
        // Timestamp
        long timestampMask = ((1 << 41) - 1) << 22; // shift = 5 + 5 + 12
        long timestamp = (id & timestampMask) >> 12;

        // DataCenterId
        long dataCenterIdMask = ((1 << 5) - 1) << 17; // shift = 5 + 12
        long dataCenterId = (id & dataCenterIdMask) >> 17;

        // MachineId
        long machineIdMask = ((1 << 5) - 1) << 12; // shift = 12;
        long machineId = (id & machineIdMask) >> 12;

        // Sequence
        long sequenceMask = (1 << 12) - 1;
        long sequence = (id & sequenceMask);

        return (timestamp, dataCenterId, machineId, sequence);
    }
}
