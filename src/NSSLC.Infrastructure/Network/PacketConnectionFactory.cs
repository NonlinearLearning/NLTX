using Terraria.Network;

namespace NSSLC.Infrastructure.Network;

public sealed class PacketConnectionFactory {
  private readonly ProtocolProfile _profile;
  private readonly PacketConnectionOptions _options;
  private readonly PacketByteBudget _budget;
  private readonly Guid _sessionKey = Guid.NewGuid();
  private long _epoch;

  public PacketConnectionFactory(ProtocolProfile profile, PacketConnectionOptions? options = null,
      PacketByteBudget? budget = null) {
    _profile = profile;
    _options = options ?? new();
    _options.Validate();
    _budget = budget ?? new();
  }

  public async ValueTask<IPacketConnection> ConnectAsync(string address, int port,
      TimeSpan connectTimeout, CancellationToken cancellationToken = default) {
    var adapter = new NetCoreServerClientAdapter(address, port, _profile, _options, _budget,
        new(_sessionKey, Interlocked.Increment(ref _epoch)));
    try {
      if (!adapter.ConnectAsync()) {
        throw new IOException("The TCP connection attempt could not start.");
      }
      await adapter.Connected.WaitAsync(connectTimeout, cancellationToken).ConfigureAwait(false);
      return new OwnedConnection(adapter);
    } catch {
      await adapter.Connection.DisposeAsync().ConfigureAwait(false);
      adapter.Dispose();
      throw;
    }
  }

  private sealed class OwnedConnection : IPacketConnection {
    private readonly NetCoreServerClientAdapter _adapter;
    private int _disposed;

    public ConnectionIdentity Identity => _adapter.Connection.Identity;
    public string ProfileKey => _adapter.Connection.ProfileKey;

    public OwnedConnection(NetCoreServerClientAdapter adapter) {
      _adapter = adapter;
    }

    public ValueTask<PacketMessage?> ReadPacketAsync(CancellationToken cancellationToken = default) {
      return _adapter.Connection.ReadPacketAsync(cancellationToken);
    }

    public ValueTask<PacketWriteReceipt> WritePacketAsync<TPacket>(TPacket packet,
        CancellationToken cancellationToken = default) {
      return _adapter.Connection.WritePacketAsync(packet, cancellationToken);
    }

    public async ValueTask DisposeAsync() {
      if (Interlocked.Exchange(ref _disposed, 1) == 0) {
        await _adapter.Connection.DisposeAsync().ConfigureAwait(false);
        _adapter.Dispose();
      }
    }
  }
}
