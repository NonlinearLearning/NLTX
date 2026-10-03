namespace NSSLC.Infrastructure.Network;

public sealed record PacketConnectionOptions {
  public int MaximumFrameBytes { get; init; } = ushort.MaxValue;
  public int ReceiveBytes { get; init; } = 128 * 1024;
  public int ReceiveItems { get; init; } = 64;
  public int SendBytes { get; init; } = 512 * 1024;
  public int SendItems { get; init; } = 128;
  public int MaximumWaitingWriters { get; init; } = 64;
  public int ControlReserveBytes { get; init; } = 16 * 1024;
  public int ControlReserveItems { get; init; } = 4;
  public TimeSpan SendTimeout { get; init; } = TimeSpan.FromSeconds(10);
  public TimeSpan PartialFrameTimeout { get; init; } = TimeSpan.FromSeconds(5);

  internal void Validate() {
    if (MaximumFrameBytes < 3 || MaximumFrameBytes > ushort.MaxValue
        || ReceiveBytes < MaximumFrameBytes || SendBytes < MaximumFrameBytes
        || ReceiveItems < 1 || SendItems < 1 || MaximumWaitingWriters < 1
        || ControlReserveBytes < 0 || ControlReserveItems < 0
        || SendTimeout <= TimeSpan.Zero || PartialFrameTimeout <= TimeSpan.Zero) {
      throw new ArgumentOutOfRangeException(nameof(PacketConnectionOptions));
    }
  }
}
