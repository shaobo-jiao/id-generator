using System.ComponentModel.DataAnnotations;

namespace IdGenerator.Api;

/// <summary>
/// Gets configuration options to create a SnowflakeIdGenerator instance
/// </summary>
public class SnowflakeIdGeneratorOptions
{
    [Range(0, SnowflakeId.MaxDataCenterId)]
    public int DataCenterId { get; set; }

    [Range(0, SnowflakeId.MaxMachineId)]
    public int MachineId { get; set; }
}
