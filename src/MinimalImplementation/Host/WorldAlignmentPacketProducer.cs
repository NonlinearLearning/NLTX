using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Protocol;
using MinimalImplementation.Server.World;
using Terraria.NetWork.Prototype.PacketDesignCompiler;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace MinimalImplementation.Server.Host;

/// <summary>Produces the Version4 packet 57 alignment percentages for active recipients.</summary>
public sealed class WorldAlignmentPacketProducer
{
  private readonly WorldOwner _worldOwner;
  private readonly Version4WireProfile _wireProfile;

  public WorldAlignmentPacketProducer(WorldOwner worldOwner, Version4WireProfile wireProfile)
  {
    ArgumentNullException.ThrowIfNull(worldOwner);
    ArgumentNullException.ThrowIfNull(wireProfile);
    _worldOwner = worldOwner;
    _wireProfile = wireProfile;
  }

  public OutboundPacketIntent CreateIntent(IReadOnlyCollection<SessionEpoch> recipients)
  {
    ArgumentNullException.ThrowIfNull(recipients);
    if (recipients.Count == 0)
    {
      throw new ArgumentException("Packet 57 requires at least one recipient.", nameof(recipients));
    }

    WorldAlignmentSnapshot snapshot = _worldOwner.CaptureAlignmentSnapshot();
    WorldAlignmentMetrics metrics = snapshot.Metrics;
    var packet = new Unknown57Packet
    {
      Good = metrics.GoodPercent,
      Evil = metrics.EvilPercent,
      Blood = metrics.BloodPercent
    };
    Version4EncodedPacket encoded = _wireProfile.Encode(WireDirection.ServerToClient, packet);
    return new OutboundPacketIntent(
      recipients,
      ignoreEpoch: null,
      encoded.MessageId,
      encoded.Payload,
      snapshot.Version);
  }
}
