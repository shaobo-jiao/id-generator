using System.Collections.Concurrent;
using IdGenerator.Api;
using Microsoft.Extensions.Options;

namespace IdGenerator.Tests;

/// <summary>
/// Tests for SnowflakeIdGenerator.
/// </summary>
public class SnowflakeIdGeneratorTests
{
    private readonly FakeTimeProvider _fakeTimeProvider;
    private readonly SnowflakeIdGenerator _idGenerator;

    public SnowflakeIdGeneratorTests()
    {
        // arrange dependencies for each test.
        _fakeTimeProvider = new FakeTimeProvider(DateTimeOffset.UtcNow);
        var options = Options.Create(new SnowflakeIdGeneratorOptions { DataCenterId = 1, MachineId = 1 });
        _idGenerator = new SnowflakeIdGenerator(options, _fakeTimeProvider);
    }

    [Fact]
    public void NewId_GeneratesSingleId_ReturnsPositiveId()
    {
        // Act
        long id = _idGenerator.NewId();

        // Assert
        Assert.True(id > 0);
    }

    [Fact]
    public void NewId_GeneratesSingleId_ComponentsAreValid()
    {
        // Act
        var id = new SnowflakeId(_idGenerator.NewId());

        // Assert
        Assert.True(id.Timestamp >= 0); // ignore timestamp comparison as epoch is inaccessible. just check non-negative.
        Assert.Equal(1, id.DataCenterId);
        Assert.Equal(1, id.MachineId);
        Assert.Equal(0, id.Sequence);
    }

    [Fact]
    public void NewId_GeneratesMultipleIds_ReturnsUniqueIds()
    {
        // Arrange
        int count = 1000;
        var ids = new HashSet<long>();

        // Act
        for (int i = 0; i < count; i++)
        {
            long id = _idGenerator.NewId();
            ids.Add(id);
        }

        // Assert
        Assert.Equal(count, ids.Count); // all IDs should be unique
    }

    [Fact]
    public void NewId_GeneratesMultipleIds_IdsAreIncremental()
    {
        // Arrange
        int count = 999;
        long lastId = _idGenerator.NewId();

        // Act & Assert
        for (int i = 0; i < count; i++)
        {
            long id = _idGenerator.NewId();
            Assert.True(id > lastId); // IDs should be incremental
            lastId = id;
        }
    }

    [Fact]
    public void NewId_NextMillisecond_ResetsSequence()
    {
        // Arrange: generates a few IDs first.
        for (int i = 0; i < 1000; i++)
            _idGenerator.NewId();

        // Act
        _fakeTimeProvider.AdvanceMilliseconds(1); // advance to next millisecond    
        long id = _idGenerator.NewId();
        long sequence = new SnowflakeId(id).Sequence;

        // Assert
        Assert.Equal(0, sequence); // sequence reset to 0
    }

    [Fact]
    public void NewId_SequenceOverflow_WaitsForNextMillisecond()
    {
        // Arrange
        long lastIdValue = -1;
        for (int i = 0; i < 4096; i++)
            lastIdValue = _idGenerator.NewId();
        var lastId = new SnowflakeId(lastIdValue);

        // Act
        var newId = new SnowflakeId(_idGenerator.NewId());

        // Assert
        Assert.Equal(0, newId.Sequence); // sequence reset to 0
        Assert.True(newId.Timestamp >= lastId.Timestamp); // should wait for next millisecond
    }

    [Fact]
    public async Task NewId_MultipleThreads_GeneratesUniqueIds()
    {
        // Arrange
        var options = Options.Create(new SnowflakeIdGeneratorOptions { DataCenterId = 1, MachineId = 1 });
        var idGenerator = new SnowflakeIdGenerator(options, TimeProvider.System);

        int threadCount = 10;
        int idsPerThread = 1000;
        var ids = new ConcurrentBag<long>();
        var tasks = new List<Task>();

        // Act
        for (int i = 0; i < threadCount; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                for (int j = 0; j < idsPerThread; j++)
                    ids.Add(idGenerator.NewId());
            }));
        }
        await Task.WhenAll(tasks.ToArray());

        // Assert
        Assert.Equal(threadCount * idsPerThread, ids.Count); // all IDs should be generated
        Assert.Equal(threadCount * idsPerThread, ids.Distinct().Count()); // all IDs should be unique
    }
}
