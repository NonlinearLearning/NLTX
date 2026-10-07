using System;
using System.Threading;
using System.Threading.Tasks;
using Terraria.Network;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;
using Terraria.Relationships;
using Terraria.WorldGeneration.Components;

namespace NSSLC.Infrastructure.Network;

/// <summary>Publishes the latest completed world tile metrics as packet 57.</summary>
public sealed class WorldTileMetricsPacketProducer
{
  private readonly PacketGateway _gateway;
  private readonly NetworkWorldOwner _worldOwner;
  private readonly EntityRuntimeId _worldRuntimeId;

  public WorldTileMetricsPacketProducer(PacketGateway gateway, NetworkWorldOwner worldOwner,
      EntityRuntimeId worldRuntimeId)
  {
    _gateway = gateway ?? throw new ArgumentNullException(nameof(gateway));
    _worldOwner = worldOwner ?? throw new ArgumentNullException(nameof(worldOwner));
    if (!worldRuntimeId.IsAssigned)
    {
      throw new ArgumentException("A tile metrics producer needs an assigned world runtime.",
          nameof(worldRuntimeId));
    }

    _worldRuntimeId = worldRuntimeId;
  }

  /// <summary>Captures a detached packet from the latest completed metrics publication.</summary>
  public ValueTask<Unknown57Packet?> CaptureAsync(
      CancellationToken cancellationToken = default)
  {
    return _worldOwner.InvokeAsync<Unknown57Packet?>(session =>
    {
      if (!session.World.TileMetrics.TryGetPublishedSnapshot(out WorldTileMetricsSnapshot snapshot))
      {
        return null;
      }

      return CreatePacket(snapshot);
    }, cancellationToken);
  }

  /// <summary>Publishes one current metrics snapshot to every active client.</summary>
  public async ValueTask<bool> PublishAsync(CancellationToken cancellationToken = default)
  {
    Unknown57Packet? packet = await CaptureAsync(cancellationToken).ConfigureAwait(false);
    if (packet is null)
    {
      return false;
    }

    var dispatch = new OutboundDispatch(packet, PacketDispatchKind.AllActive,
        allowedStages: NetworkSessionStage.Active, worldKey: _worldRuntimeId.Value);
    return await _gateway.PublishWorldAsync(_worldRuntimeId, dispatch, cancellationToken)
        .ConfigureAwait(false);
  }

  /// <summary>Projects a published metrics snapshot without recalculating its percentages.</summary>
  private static Unknown57Packet CreatePacket(WorldTileMetricsSnapshot snapshot)
  {
    if (snapshot.GoodPercent > 100 || snapshot.EvilPercent > 100 || snapshot.BloodPercent > 100)
    {
      throw new InvalidOperationException(
          "Published world tile metrics must contain percentages from 0 through 100.");
    }

    return new Unknown57Packet
    {
      Good = snapshot.GoodPercent,
      Evil = snapshot.EvilPercent,
      Blood = snapshot.BloodPercent
    };
  }
}
