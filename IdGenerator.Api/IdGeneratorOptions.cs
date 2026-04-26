using System.ComponentModel.DataAnnotations;

namespace IdGenerator.Api;

/// <summary>
/// Gets configuration options to create a IdGenerator instance
/// </summary>
public class IdGeneratorOptions
{
    [Range(0, 31)]
    public int DataCenterId { get; set; }

    [Range(0, 31)]
    public int MachineId { get; set; }
}
