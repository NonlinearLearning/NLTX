using System.Net.Sockets;
using MinimalImplementation.Contracts;
using Terraria.NetWork.Prototype.PacketDesignCompiler;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace MinimalImplementation.Server.Protocol;

/// <summary>Connects one framed TCP session to the Version4 server-side packet profile.</summary>
public sealed class Version4PacketConnection : IAsyncDisposable
{
  private readonly Version4WireProfile _wireProfile;
  private readonly PacketTcpConnection _transport;
  private readonly Func<InboundRawFrame, InboundPacketEnvelope> _createEnvelope;
  private readonly Func<InboundVersion4Packet, CancellationToken, ValueTask> _onPacket;
  private readonly bool _enableDebugCommands;

  public Version4PacketConnection(
    Socket socket,
    SessionEpoch epoch,
    Version4WireProfile wireProfile,
    Func<InboundRawFrame, InboundPacketEnvelope> createEnvelope,
    Func<InboundVersion4Packet, CancellationToken, ValueTask> onPacket,
    bool enableDebugCommands = false,
    PacketTcpConnectionOptions? options = null,
    TimeProvider? timeProvider = null,
    Func<ConnectionClosed, ValueTask>? onClosed = null)
  {
    ArgumentNullException.ThrowIfNull(socket);
    ArgumentNullException.ThrowIfNull(wireProfile);
    ArgumentNullException.ThrowIfNull(createEnvelope);
    ArgumentNullException.ThrowIfNull(onPacket);
    _wireProfile = wireProfile;
    _createEnvelope = createEnvelope;
    _onPacket = onPacket;
    _enableDebugCommands = enableDebugCommands;
    _transport = new PacketTcpConnection(
      socket,
      epoch,
      DispatchFrameAsync,
      options,
      timeProvider,
      onClosed);
  }

  public SessionEpoch Epoch => _transport.Epoch;

  public bool IsRunning => _transport.IsRunning;

  public long LastReceivedSequence => _transport.LastReceivedSequence;

  public TransportSendCertainty LastSendCertainty => _transport.LastSendCertainty;

  public Task RunAsync(CancellationToken cancellationToken)
  {
    return _transport.RunAsync(cancellationToken);
  }

  public OutboundPacketIntent CreateOutboundIntent(
    object packet,
    CommitVersion commitVersion,
    bool enableDebugCommands = false)
  {
    return _wireProfile.CreateOutboundIntent(
      packet,
      [Epoch],
      ignoreEpoch: null,
      commitVersion: commitVersion,
      enableDebugCommands: enableDebugCommands);
  }

  public ValueTask<SendReceipt> SendAsync(
    OutboundPacketIntent intent,
    CancellationToken cancellationToken)
  {
    return _transport.SendAsync(intent, cancellationToken);
  }

  public void RequestClose(string reason)
  {
    _transport.RequestClose(reason);
  }

  public ValueTask DisposeAsync()
  {
    return _transport.DisposeAsync();
  }

  private ValueTask DispatchFrameAsync(InboundRawFrame frame, CancellationToken cancellationToken)
  {
    InboundPacketEnvelope envelope = _createEnvelope(frame);
    InboundVersion4Packet packet = _wireProfile.DecodeInbound(
      frame,
      envelope,
      _enableDebugCommands);
    return _onPacket(packet, cancellationToken);
  }
}
