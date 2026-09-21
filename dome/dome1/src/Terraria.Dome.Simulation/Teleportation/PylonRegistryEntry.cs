using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.Teleportation;

public readonly record struct PylonRegistryEntry(
  WorldTileCoordinate Position,
  byte Kind,
  int PersistentId,
  bool IsValid)
{
  public bool CanTeleport => IsValid && Kind != 0;
}
