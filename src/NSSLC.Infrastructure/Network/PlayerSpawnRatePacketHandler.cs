using System.Collections.Concurrent;
using Terraria.Network;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace NSSLC.Infrastructure.Network;

/// <summary>Records the personal spawn-rate slider for the headless host probe.</summary>
public sealed class PlayerSpawnRatePacketHandler : IPacketHandler<NetModulesPacket> {
  private readonly ConcurrentDictionary<byte, float> _observedSliders = new();

  public IReadOnlyDictionary<byte, float> Snapshot() {
    return new Dictionary<byte, float>(_observedSliders);
  }

  public ValueTask<PacketHandlingResult> HandleAsync(NetworkSessionContext context,
      NetModulesPacket packet, CancellationToken cancellationToken) {
    cancellationToken.ThrowIfCancellationRequested();
    if (packet.Data is not Packet82CreativePowerData { PowerId: 14,
            Value: Packet82PerPlayerSliderPowerState slider }
        || !float.IsFinite(slider.Value) || slider.Value is < 0 or > 1) {
      return ValueTask.FromResult(new PacketHandlingResult(false,
          rejectionCode: "UnsupportedCreativePower"));
    }

    _observedSliders[context.Actor.PlayerSlot] = slider.Value;
    return ValueTask.FromResult(new PacketHandlingResult(true));
  }
}
