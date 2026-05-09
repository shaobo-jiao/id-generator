namespace IdGen.Api;

/// <summary>
/// Represents the structure of a Snowflake Id. The structure is configurable but suggested not to 
/// change once determined.
/// </summary>
public class IdStructure(int timestampBits, int workerIdBits, int sequenceBits)
{
    public int TimestampBits { get; } = timestampBits;
    public int WorkerIdBits { get; } = workerIdBits;
    public int SequenceBits { get; } = sequenceBits;

    public long MaxTimestamp => (1L << TimestampBits) - 1;
    public int MaxWorkerId => (1 << WorkerIdBits) - 1;
    public int MaxSequence => (1 << SequenceBits) - 1;
}
