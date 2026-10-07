using System.Net;
using Terraria.Relationships;
using Terraria.Network;

namespace NSSLC.Infrastructure.Network;

public sealed class NetworkGatewayHost : IAsyncDisposable {
  private readonly PacketTcpServer _server;
  private int _disposed;

  public ProtocolProfile Profile { get; }
  public PacketByteBudget Budget { get; }
  public PacketSnapshotCache Snapshots { get; }
  public PacketGateway Gateway { get; }
  public PacketConnectionFactory Connections { get; }
  public EndPoint EndPoint => _server.EndPoint;
  public Exception? LastTransportError => _server.LastError;

  public NetworkGatewayHost(IPAddress address, int port, ProtocolFacts facts,
      INetworkSessionAuthority authority, PacketGatewayOptions? gatewayOptions = null,
      PacketConnectionOptions? connectionOptions = null, PacketByteBudget? budget = null,
      TimeProvider? timeProvider = null,
      Func<EntityRuntimeId?>? worldRuntimeIdProvider = null,
      Func<NetworkSessionContext, CancellationToken, ValueTask>? sessionClosing = null)
      : this(address, port, TerrariaProtocolProfile.Create(facts),
          authority, gatewayOptions, connectionOptions, budget, timeProvider,
          worldRuntimeIdProvider, sessionClosing) {
  }

  public NetworkGatewayHost(IPAddress address, int port, ProtocolProfile profile,
      INetworkSessionAuthority authority, PacketGatewayOptions? gatewayOptions = null,
      PacketConnectionOptions? connectionOptions = null, PacketByteBudget? budget = null,
      TimeProvider? timeProvider = null,
      Func<EntityRuntimeId?>? worldRuntimeIdProvider = null,
      Func<NetworkSessionContext, CancellationToken, ValueTask>? sessionClosing = null) {
    ArgumentNullException.ThrowIfNull(profile);
    Profile = profile;
    Budget = budget ?? new();
    Snapshots = new(Profile, Budget, timeProvider: timeProvider);
    Gateway = new(
      Profile,
      authority,
      gatewayOptions,
      timeProvider,
      Snapshots,
      worldRuntimeIdProvider,
      sessionClosing);
    Connections = new(Profile, connectionOptions, Budget);
    _server = new(address, port, Profile, Gateway.HandleAsync, connectionOptions, Budget);
  }

  public void Start() {
    ObjectDisposedException.ThrowIf(Volatile.Read(ref _disposed) != 0, this);
    Gateway.Seal();
    _server.Start();
  }

  public async ValueTask DisposeAsync() {
    if (Interlocked.Exchange(ref _disposed, 1) != 0) {
      return;
    }
    try {
      await _server.DisposeAsync().ConfigureAwait(false);
    } finally {
      try {
        await Gateway.DisposeAsync().ConfigureAwait(false);
      } finally {
        Snapshots.Dispose();
      }
    }
  }
}
