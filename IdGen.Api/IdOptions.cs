namespace IdGen.Api;

/// <summary>
/// Configures the Id structure and current IdGenerator WorkerId.
/// </summary>
public class IdOptions
{
    /// <summary>
    /// Defines the custom epoch. This field should never be changed once commenced.
    /// </summary>
    public DateTimeOffset Epoch { get; set; }

    /// <summary>
    /// Defines the number of timestamp bits in the ID. 
    /// This field should never be changed once commenced. 
    /// </summary>
    public int TimestampBits { get; set; }

    /// <summary>
    /// Defines the number of WorkerId bits in the ID. 
    /// This field should never be changed once commenced. 
    /// </summary>
    public int WorkerIdBits { get; set; }
    
    /// <summary>
    /// Defines the number of Sequence bits in the ID. 
    /// This field should never be changed once commenced. 
    /// </summary>
    public int SequenceBits { get; set; }

    /// <summary>
    /// Configures the WorkId of current IdGenerator instance.
    /// </summary>
    public int WorkerId { get; set; }
}
