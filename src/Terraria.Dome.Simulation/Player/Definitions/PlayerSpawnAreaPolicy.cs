using System;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldModel.Definitions;

namespace Terraria.Dome.Simulation.Player.Definitions;

public static class PlayerSpawnAreaPolicy
{
  private const int HorizontalRadius = 1;
  private const int AboveSpawnTiles = 3;

  private static readonly TileDefinitionRegistry DefaultTileDefinitions =
    TileDefinitionRegistry.CreateVersion4Base();

  public static bool IsValid(WorldGrid world, SimulationVector position)
  {
    ArgumentNullException.ThrowIfNull(world);
    if (!float.IsFinite(position.X) || !float.IsFinite(position.Y))
    {
      return false;
    }

    int floorX = (int)MathF.Floor(position.X);
    int floorY = (int)MathF.Floor(position.Y);
    for (int x = floorX - HorizontalRadius; x <= floorX + HorizontalRadius; x++)
    {
      for (int y = floorY - AboveSpawnTiles; y < floorY; y++)
      {
        if (!world.Contains(x, y))
        {
          continue;
        }

        WorldTile tile = world.GetTile(x, y);
        if (tile.LiquidAmount > 0)
        {
          return false;
        }

        // The tile immediately below the spawn position is the support surface,
        // not part of the open space that must be cleared for the player body.
        if (y == floorY - 1)
        {
          continue;
        }

        if (TileStateQuery.IsSolidWithoutPlatformTop(tile, DefaultTileDefinitions))
        {
          return false;
        }
      }
    }

    return true;
  }
}
