namespace Terraria.Network.Transport;

public sealed class NetworkBufferLease
{
  internal NetworkBufferLease(
    NetworkBufferPoolAdapter owner,
    byte[] buffer,
    NetworkBufferBucket bucket,
    int requestedLength)
  {
    Owner = owner;
    Buffer = buffer;
    Bucket = bucket;
    RequestedLength = requestedLength;
  }

  public byte[] Buffer { get; }

  public NetworkBufferBucket Bucket { get; }

  public int RequestedLength { get; }

  internal NetworkBufferPoolAdapter Owner { get; }

  internal bool IsReturned { get; set; }
}
