namespace VietnamTrafficPolice.WebApi.Storage.Ef;

public class DistributedCache
{
    public string Id { get; set; } = null!;

    public DateTime? AbsoluteExpiration { get; set; }

    public DateTime ExpiresAtTime { get; set; }

    public long? SlidingExpirationInSeconds { get; set; }

    public byte[] Value { get; set; } = null!;
}