using IdGenerator.Api;
using Microsoft.Extensions.Options;

namespace IdGenerator.Tests;

/// <summary>
/// Tests for IdGenerator.
/// </summary>
public class IdGeneratorTests
{
    [Fact]
    public void NewId_GeneratesSingleId_ReturnsPositiveId()
    {
        // Arrange
        var options = Options.Create(new IdGeneratorOptions { DataCenterId = 1, MachineId = 1 });
        var fakeTimeProvider = new FakeTimeProvider(DateTimeOffset.UtcNow);
        var idGenerator = new IdGenerator.Api.IdGenerator(options, fakeTimeProvider);

        // Act
        long id = idGenerator.NewId();

        // Assert
        Assert.True(id > 0);
    }

    [Fact]
    public void NewId_GeneratesMultipleIds_ReturnsUniqueIds()
    {
        // Arrange
        var options = Options.Create(new IdGeneratorOptions { DataCenterId = 1, MachineId = 1 });
        var fakeTimeProvider = new FakeTimeProvider(DateTimeOffset.UtcNow);
        var idGenerator = new IdGenerator.Api.IdGenerator(options, fakeTimeProvider);
        int count = 1000;
        var ids = new HashSet<long>();

        // Act
        for (int i = 0; i < count; i++)
        {
            long id = idGenerator.NewId();
            ids.Add(id);
        }

        // Assert
        Assert.Equal(count, ids.Count); // all IDs should be unique
    }

    [Fact]
    public void NewId_GeneratesMultipleIds_IdsAreIncreasing()
    {
        // Arrange
        var options = Options.Create(new IdGeneratorOptions { DataCenterId = 1, MachineId = 1 });
        var fakeTimeProvider = new FakeTimeProvider(DateTimeOffset.UtcNow);
        var idGenerator = new IdGenerator.Api.IdGenerator(options, fakeTimeProvider);
        int count = 1000;
        long lastId = -1;

        // Act & Assert
        for (int i = 0; i < count; i++)
        {
            long id = idGenerator.NewId();
            if (lastId != -1)
            {
                Assert.True(id > lastId); // IDs should be increasing
            }
            lastId = id;
        }
    }

    [Fact]
    public void NewId_NextMillisecond_ResetsSequence()
    {
        // Arrange
        var options = Options.Create(new IdGeneratorOptions { DataCenterId = 1, MachineId = 1 });
        var fakeTimeProvider = new FakeTimeProvider(DateTimeOffset.UtcNow);
        var idGenerator = new IdGenerator.Api.IdGenerator(options, fakeTimeProvider);
        for (int i = 0; i < 1000; i++)
            idGenerator.NewId();
        long sequenceMask = (1 << 12) - 1;

        // Act
        fakeTimeProvider.AdvanceMilliseconds(1); // advance to next millisecond    
        long id = idGenerator.NewId();
        long sequence = id & sequenceMask;

        // Assert
        Assert.Equal(0, sequence); // sequence reset to 0
    }

    [Fact]
    public void NewId_SequenceOverflow_WaitsForNextMillisecond()
    {
        // Arrange
        var options = Options.Create(new IdGeneratorOptions { DataCenterId = 1, MachineId = 1 });
        var fakeTimeProvider = new FakeTimeProvider(DateTimeOffset.UtcNow);
        var idGenerator = new IdGenerator.Api.IdGenerator(options, fakeTimeProvider);

        long lastId = -1;
        for (int i = 0; i < 4096; i++) lastId = idGenerator.NewId();

        long timestampMask = ((1L << 41) - 1) << 22; // shift = 5 + 5 + 12 = 22
        long lastTimestamp = (lastId & timestampMask) >> 22;

        // Act
        long newId = idGenerator.NewId();
        long timestamp = (newId & timestampMask) >> 22;
        long sequence = newId & ((1 << 12) - 1);
        
        // Assert
        Assert.Equal(0, sequence); // sequence reset to 0
        Assert.True(timestamp >= lastTimestamp); // should wait for next millisecond
    }
}
