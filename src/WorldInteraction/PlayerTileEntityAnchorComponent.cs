using Terraria.Relationships;
using Terraria.WorldInteraction.Tiles;

namespace Terraria.WorldInteraction;

// Stores the player's relation to an external TileEntity by identity and tile.
// Registry lookup, validity checks and network projection remain adapters.
public sealed class PlayerTileEntityAnchorComponent
{
  public EntityReference? AnchorEntity { get; set; }

  public TileCoordinate? AnchorCoordinate { get; set; }

  public bool HasAnchor =>
    AnchorEntity.HasValue &&
    !AnchorEntity.Value.IsEmpty &&
    AnchorCoordinate.HasValue;
}
