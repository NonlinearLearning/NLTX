using Terraria.Relationships;
using Terraria.WorldInteraction.Tiles;

namespace Terraria.Teleportation;

public readonly record struct TeleportEndpointSnapshot(
  EntityReference Endpoint,
  TileCoordinate Position,
  uint Revision,
  bool IsActive)
{
  public bool IsValid =>
    !Endpoint.IsEmpty &&
    Position.X >= 0 &&
    Position.Y >= 0;
}
