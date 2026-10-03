using Terraria.Network;

namespace NSSLC.NetworkVerification;

internal sealed class StubPacketConnection : IPacketConnection {
  private int _disposed;
  private int _writes;

  public ConnectionIdentity Identity { get; }
  public string ProfileKey => "reconnect-verification";
  public int Disposals => Volatile.Read(ref _disposed);
  public int Writes => Volatile.Read(ref _writes);

  public StubPacketConnection(ConnectionIdentity identity) {
    Identity = identity;
  }

  public ValueTask<PacketMessage?> ReadPacketAsync(CancellationToken cancellationToken = default) {
    throw new NotSupportedException("Reconnect policy must leave packet rebuilding to its caller.");
  }

  public ValueTask<PacketWriteReceipt> WritePacketAsync<TPacket>(TPacket packet,
      CancellationToken cancellationToken = default) {
    Interlocked.Increment(ref _writes);
    throw new NotSupportedException("Reconnect policy must not automatically replay packets.");
  }

  public ValueTask DisposeAsync() {
    Interlocked.Increment(ref _disposed);
    return ValueTask.CompletedTask;
  }
}
