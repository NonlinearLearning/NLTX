using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyExtraLiquidAddLiquid
{
  private const int BorderMargin = 40;
  private const ushort HoneyWallType = 86;
  private const ushort FirstWetWallType = 187;
  private const ushort SecondWetWallType = 216;
  private const byte HoneyLiquidType = 2;
  private const string Source = "worldgen.secretseed.ExtraLiquid.addLiquid";

  public static void AppendCommands(
    WorldGridSnapshot snapshot,
    int worldSurfaceY,
    int underworldLayerY,
    bool isRemixWorld,
    bool isSkyblockWorld,
    LegacyPassRandomState random,
    ref WorldGenerationStateComponent state,
    List<LiquidChangeCommand> commands)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(random);
    ArgumentNullException.ThrowIfNull(commands);
    if (worldSurfaceY < 0 || worldSurfaceY >= snapshot.Metadata.Height ||
        underworldLayerY < 0 || underworldLayerY >= snapshot.Metadata.Height)
    {
      throw new ArgumentOutOfRangeException(nameof(worldSurfaceY));
    }

    if (isSkyblockWorld)
    {
      return;
    }

    for (int x = BorderMargin; x < snapshot.Metadata.Width - BorderMargin; x++)
    {
      for (int y = BorderMargin; y < snapshot.Metadata.Height - BorderMargin; y++)
      {
        WorldTile tile = snapshot.GetTile(x, y);
        if (tile.IsActive)
        {
          continue;
        }

        AppendCommandForTile(
          tile, x, y, worldSurfaceY, underworldLayerY, isRemixWorld, random, ref state, commands);
      }
    }
  }

  private static void AppendCommandForTile(
    WorldTile tile,
    int x,
    int y,
    int worldSurfaceY,
    int underworldLayerY,
    bool isRemixWorld,
    LegacyPassRandomState random,
    ref WorldGenerationStateComponent state,
    List<LiquidChangeCommand> commands)
  {
    if (tile.WallType == 0 && y < worldSurfaceY)
    {
      AppendWaterWhen(random.Next(3) == 0, tile, x, y, ref state, commands);
      return;
    }

    if (y > underworldLayerY)
    {
      AppendWaterWhen(!isRemixWorld && random.Next(4) == 0, tile, x, y, ref state, commands);
      return;
    }

    if (tile.WallType is FirstWetWallType or SecondWetWallType)
    {
      AppendWaterWhen(random.Next(3) == 0, tile, x, y, ref state, commands);
      return;
    }

    if (random.Next(3) != 0)
    {
      byte liquidType = tile.LiquidAmount == 0 && tile.WallType == HoneyWallType
        ? HoneyLiquidType
        : tile.LiquidType;
      commands.Add(new LiquidChangeCommand(
        state.ReserveSequence(), x, y, byte.MaxValue, liquidType, Source: Source));
    }
  }

  private static void AppendWaterWhen(
    bool condition,
    WorldTile tile,
    int x,
    int y,
    ref WorldGenerationStateComponent state,
    List<LiquidChangeCommand> commands)
  {
    if (condition)
    {
      commands.Add(new LiquidChangeCommand(
        state.ReserveSequence(), x, y, byte.MaxValue, tile.LiquidType, Source: Source));
    }
  }
}
