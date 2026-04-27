using System.ComponentModel.DataAnnotations;

namespace IdGenerator.Api;

/// <summary>
/// Gets configuration options to create a IdGenerator instance
/// </summary>
public class IdGeneratorOptions
{
    [Range(0, Id.MaxDataCenterId)]
    public int DataCenterId { get; set; }

    [Range(0, Id.MaxMachineId)]
    public int MachineId { get; set; }
}
