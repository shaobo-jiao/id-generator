using Microsoft.Extensions.Options;
using Microsoft.Extensions.Validation;

namespace IdGen.Api;

/// <summary>
/// Validates IdOptions
/// </summary>
public class IdOptionsValidator : IValidateOptions<IdOptions>
{
    private const int MinExpiryYears = 10; // validate timestamp fields is not too short and causes IdGenerator expires quickly

    public ValidateOptionsResult Validate(string? name, IdOptions options)
    {
        // epoch 
        if (options.Epoch > DateTimeOffset.UtcNow)
            return ValidateOptionsResult.Fail("Epoch cannot be a future time point.");

        // timestamp fields
        if (options.TimestampBits <= 0)
            return ValidateOptionsResult.Fail("TimestampBits cannot be negative");

        // worker id
        if (options.WorkerIdBits <= 0)
            return ValidateOptionsResult.Fail("WorkerIdBits cannot be negative");
        
        // sequence
        if (options.SequenceBits <= 0)
            return ValidateOptionsResult.Fail("SequenceBits cannot be negative");
        
        // total number of bits
        int totalBits = 1 + options.TimestampBits + options.WorkerIdBits + options.SequenceBits;
        if (totalBits != 64)
            return ValidateOptionsResult.Fail("Total number of bits must be 64 (including 1st sign bit)");
        
        // timestamp not too small
        long maxTimestamp = (1L << options.TimestampBits) - 1;
        DateTimeOffset expiryDate;
        try
        {
            expiryDate = options.Epoch.AddMilliseconds(maxTimestamp);
        }
        catch (ArgumentOutOfRangeException)
        {
            expiryDate = DateTimeOffset.MaxValue;
        }
        if (expiryDate <= DateTimeOffset.UtcNow.AddYears(MinExpiryYears))
            return ValidateOptionsResult.Fail($"Timestamp range is too short. Expiry Date: {expiryDate}");

        // WorkerId
        long maxWorkerId = (1 << options.WorkerId) - 1;
        if (options.WorkerId < 0 || options.WorkerId > maxWorkerId)
            return ValidateOptionsResult.Fail($"WorkId is not in range [0, {maxWorkerId}]");

        return ValidateOptionsResult.Success;
    }
}
