using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.Items.Definitions;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.Items.Systems;

public sealed class ItemPlacementSystem
{
  public bool TryCreateTileCommand(
    ItemDefinition definition,
    int x,
    int y,
    long sequence,
    out TileChangeCommand command,
    out ItemCommandRejection rejection)
  {
    if (sequence < 0)
    {
      command = default;
      rejection = ItemCommandRejection.Invalid(
        "Item placement requires a non-negative command sequence.");
      return false;
    }

    if (definition.Placement is not ItemPlacementDefinition placement)
    {
      command = default;
      rejection = ItemCommandRejection.Invalid(
        "The item has no tile or wall placement definition.");
      return false;
    }

    bool hasTile = placement.TileType >= 0;
    bool hasWall = placement.WallType >= 0;
    if (hasTile == hasWall)
    {
      command = default;
      rejection = ItemCommandRejection.Invalid(
        "The item must define exactly one tile or wall placement type.");
      return false;
    }

    command = new TileChangeCommand(
      sequence,
      x,
      y,
      hasTile ? TileChangeKind.Place : TileChangeKind.SetWall,
      hasTile ? (ushort)placement.TileType : (ushort)0,
      hasWall ? (ushort)placement.WallType : (ushort)0,
      IsCartTrack: placement.CartTrack);
    rejection = default;
    return true;
  }
}
