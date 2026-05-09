namespace IdGen.Api;

/// <summary>
/// wraps an Id generated from IdGenerator
/// 
/// As an example below, the bits of an ID are partitioned as follows: 
/// - bits[0]: sign bit, always 0
/// - bits[1..41]: timestamp bits representing milliseconds since a custom epoch, 41 bits
/// - bits[42..51]: worker ID, 10 bits
/// - bits[52..63]: sequence number, 12 bits
/// 
/// The actual number of bits are configurable via IdOptions.
/// </summary>
public class Id
{
    public IdStructure IdStructure { get; }

    public long Value { get; }
    public long Timestamp { get; }
    public long WorkerId { get; }
    public long Sequence { get; }

    public Id(IdStructure idStructure, long value)
    {
        IdStructure = idStructure;

        Value = value;
        (Timestamp, WorkerId, Sequence) = DecomposeId(value);
    }

    public Id(IdStructure idStructure, long timestamp, long workerId, long sequence)
    {
        IdStructure = idStructure;

        Timestamp = timestamp;
        WorkerId = workerId;
        Sequence = sequence;
        Value = ComposeId(timestamp, workerId, sequence);
    }

    private long ComposeId(long timestamp, long workerId, long sequence)
    {
        int timestampShifts = IdStructure.WorkerIdBits + IdStructure.SequenceBits;
        int workerIdShifts = IdStructure.SequenceBits;

        return (timestamp << timestampShifts)
            | (workerId << workerIdShifts)
            | sequence;
    }

    private (long timestamp, long workerId, long sequence) DecomposeId(long id)
    {
        // Timestamp
        int timestampShifts = IdStructure.WorkerIdBits + IdStructure.SequenceBits;
        long timestampMask = ((1 << IdStructure.TimestampBits) - 1) << timestampShifts;
        long timestamp = (id & timestampMask) >> timestampShifts;

        // WorkerId
        int workerIdShifts = IdStructure.SequenceBits;
        long workerIdMask = ((1 << IdStructure.WorkerIdBits) - 1) << workerIdShifts;
        long workerId = (id & workerIdMask) >> workerIdShifts;

        // Sequence
        long sequenceMask = (1 << IdStructure.SequenceBits) - 1;
        long sequence = id & sequenceMask;

        return (timestamp, workerId, sequence);
    }
}
