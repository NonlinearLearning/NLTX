using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyExtraLiquidFinish
{
  private const ushort FirstFullbrightRemovalTileType = 373;
  private const ushort SecondFullbrightRemovalTileType = 374;
  private const ushort ThirdFullbrightRemovalTileType = 375;
  private const ushort FourthFullbrightRemovalTileType = 709;
  private const ushort FirstUnderworldRemovalTileType = 56;
  private const ushort SecondUnderworldRemovalTileType = 230;
  private const ushort ThirdUnderworldRemovalTileType = 659;
  private const byte LavaLiquidType = 1;
  private const string Source = "worldgen.secretseed.ExtraLiquid.finish";

  public static void AppendCommands(
    WorldGridSnapshot snapshot,
    int underworldLayerY,
    bool isSkyblockWorld,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> tileCommands,
    List<LiquidChangeCommand> liquidCommands)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(tileCommands);
    ArgumentNullException.ThrowIfNull(liquidCommands);
    if (underworldLayerY < 0 || underworldLayerY >= snapshot.Metadata.Height)
    {
      throw new ArgumentOutOfRangeException(nameof(underworldLayerY));
    }

    if (isSkyblockWorld)
    {
      return;
    }

    for (int x = 0; x < snapshot.Metadata.Width; x++)
    {
      for (int y = 0; y < snapshot.Metadata.Height; y++)
      {
        WorldTile tile = snapshot.GetTile(x, y);
        if (tile.IsFullbrightBlock)
        {
          liquidCommands.Add(new LiquidChangeCommand(
            state.ReserveSequence(), x, y, 0, tile.LiquidType, Source: Source));
          tileCommands.Add(new TileChangeCommand(
            state.ReserveSequence(), x, y, TileChangeKind.SetCoating, 0,
            IsFullbrightBlock: false, Source: Source));
          tile = tile with { LiquidAmount = 0, IsFullbrightBlock = false };
          if (IsFullbrightRemovalTile(tile.Type))
          {
            tileCommands.Add(new TileChangeCommand(
              state.ReserveSequence(), x, y, TileChangeKind.Kill, 0, Source: Source));
            tile = default;
          }
        }

        if (y >= underworldLayerY)
        {
          if (tile.LiquidAmount > 0)
          {
            liquidCommands.Add(new LiquidChangeCommand(
              state.ReserveSequence(), x, y, tile.LiquidAmount, LavaLiquidType, Source: Source));
          }

          if (IsUnderworldRemovalTile(tile.Type))
          {
            tileCommands.Add(new TileChangeCommand(
              state.ReserveSequence(), x, y, TileChangeKind.Kill, 0, Source: Source));
          }
        }
      }
    }
  }

  private static bool IsFullbrightRemovalTile(ushort tileType)
  {
    return tileType is FirstFullbrightRemovalTileType or SecondFullbrightRemovalTileType or
      ThirdFullbrightRemovalTileType or FourthFullbrightRemovalTileType;
  }

  private static bool IsUnderworldRemovalTile(ushort tileType)
  {
    return tileType is FirstUnderworldRemovalTileType or SecondUnderworldRemovalTileType or
      ThirdUnderworldRemovalTileType;
  }
}
