using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;
using Terraria.Network;
using Terraria.NonAuthoritative.Persistence;
using Terraria.NonAuthoritative.Simulation;
using Terraria.Relationships;
using Terraria.WorldSession.Components;

namespace NSSLC.Infrastructure.Network;

/// <summary>Publishes the authoritative world clock as server-originated packet 18.</summary>
public sealed class WorldTimePacketProducer
{
  private readonly PacketGateway _gateway;
  private readonly NetworkWorldOwner _worldOwner;
  private readonly EntityRuntimeId _worldRuntimeId;

  public WorldTimePacketProducer(PacketGateway gateway, NetworkWorldOwner worldOwner,
      EntityRuntimeId worldRuntimeId)
  {
    _gateway = gateway ?? throw new ArgumentNullException(nameof(gateway));
    _worldOwner = worldOwner ?? throw new ArgumentNullException(nameof(worldOwner));
    if (!worldRuntimeId.IsAssigned)
    {
      throw new ArgumentException("A world time producer needs an assigned world runtime.",
          nameof(worldRuntimeId));
    }

    _worldRuntimeId = worldRuntimeId;
  }

  /// <summary>Captures the current clock from the real world owner without making a second store.</summary>
  public ValueTask<SetTimePacket> CaptureAsync(CancellationToken cancellationToken = default)
  {
    return _worldOwner.InvokeAsync(session => CreatePacket(
        WorldClockSnapshotValue.Capture(session.World.TimeWeather)), cancellationToken);
  }

  /// <summary>Captures and publishes one authoritative snapshot to every active client.</summary>
  public async ValueTask<bool> PublishAsync(CancellationToken cancellationToken = default)
  {
    SetTimePacket packet = await CaptureAsync(cancellationToken).ConfigureAwait(false);
    var dispatch = new OutboundDispatch(packet, PacketDispatchKind.AllActive,
        allowedStages: NetworkSessionStage.Active, worldKey: _worldRuntimeId.Value);
    return await _gateway.PublishWorldAsync(_worldRuntimeId, dispatch, cancellationToken)
        .ConfigureAwait(false);
  }

  /// <summary>Projects the formal world clock into the Version4 packet-18 wire fields.</summary>
  public static SetTimePacket CreatePacket(WorldTimeWeatherState state)
  {
    return CreatePacket(WorldClockSnapshotValue.Capture(state));
  }

  public static SetTimePacket CreatePacket(WorldClockSnapshotValue state)
  {
    int cycleLength = state.DayTime
        ? WorldSimulationClockSystem.DayLength
        : WorldSimulationClockSystem.NightLength;
    if (!double.IsFinite(state.Time) || state.Time < 0 || state.Time > cycleLength)
    {
      throw new InvalidOperationException("The active world clock cannot be represented by packet 18.");
    }

    return new SetTimePacket
    {
      DayTime = state.DayTime ? (byte)1 : (byte)0,
      Time = checked((int)state.Time),
      SunModY = state.SunModY,
      MoonModY = state.MoonModY
    };
  }
}
