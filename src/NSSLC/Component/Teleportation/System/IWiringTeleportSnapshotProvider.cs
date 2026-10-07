using Terraria.WorldInteraction.Wiring;

namespace Terraria.Teleportation;

public interface IWiringTeleportSnapshotProvider
{
  bool TryGetSnapshot(
    in WiringTeleportCommand command,
    out TeleportTransitionSnapshot snapshot);
}
