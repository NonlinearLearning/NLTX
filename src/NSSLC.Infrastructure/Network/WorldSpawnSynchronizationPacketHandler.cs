using Terraria.Network;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace NSSLC.Infrastructure.Network;

/// <summary>Composes the initial world section transfer with the authoritative time snapshot.</summary>
public sealed class WorldSpawnSynchronizationPacketHandler : IPacketHandler<SpawnTileDataPacket>
{
  private readonly WorldSynchronizationPacketHandlers _sections;
  private readonly WorldTimePacketProducer _time;
  private readonly WorldTileMetricsPacketProducer? _tileMetrics;

  public WorldSpawnSynchronizationPacketHandler(WorldSynchronizationPacketHandlers sections,
      WorldTimePacketProducer time, WorldTileMetricsPacketProducer? tileMetrics = null)
  {
    _sections = sections ?? throw new ArgumentNullException(nameof(sections));
    _time = time ?? throw new ArgumentNullException(nameof(time));
    _tileMetrics = tileMetrics;
  }

  public async ValueTask<PacketHandlingResult> HandleAsync(NetworkSessionContext context,
      SpawnTileDataPacket packet, CancellationToken cancellationToken)
  {
    PacketHandlingResult result = await _sections.HandleAsync(context, packet, cancellationToken)
        .ConfigureAwait(false);
    if (!result.Accepted)
    {
      return result;
    }

    SetTimePacket snapshot = await _time.CaptureAsync(cancellationToken).ConfigureAwait(false);
    var outbound = result.Outbound.ToList();
    outbound.Add(new OutboundDispatch(snapshot, PacketDispatchKind.Single,
        [context.Connection], allowedStages: NetworkSessionStage.Synchronizing));
    if (_tileMetrics is not null)
    {
      Unknown57Packet? metrics = await _tileMetrics.CaptureAsync(cancellationToken)
          .ConfigureAwait(false);
      if (metrics is not null)
      {
        outbound.Add(new OutboundDispatch(metrics, PacketDispatchKind.Single,
            [context.Connection], allowedStages: NetworkSessionStage.Synchronizing));
      }
    }

    return new PacketHandlingResult(true, outbound, result.NextStage, result.Interest,
        result.RejectionCode);
  }
}
