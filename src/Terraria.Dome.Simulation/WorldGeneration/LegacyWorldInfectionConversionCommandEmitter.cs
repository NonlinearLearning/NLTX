using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyWorldInfectionConversionCommandEmitter
{
  public static void AppendCommands(
    WorldGridSnapshot snapshot,
    LegacyWorldInfectionConversionInput input,
    LegacyPassRandomState random,
    ref WorldGenerationStateComponent state,
    List<LegacyWorldInfectionConversionCommand> commands)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(random);
    ArgumentNullException.ThrowIfNull(commands);
    input.Validate(snapshot);
    if (input.SkyblockWorld)
    {
      return;
    }

    for (int x = 0; x < snapshot.Metadata.Width; x++)
    {
      LegacyWorldInfectionColumn column = LegacyWorldInfectionPolicy.CreateColumn(
        x,
        snapshot.Metadata.Width,
        input.WorldSurfaceY,
        input.RockLayerY,
        input.UnderworldLayerY,
        input.NoInfection,
        input.NoSurface,
        input.HallowOnSurface,
        input.DrunkWorld,
        input.Crimson,
        input.CrimsonLeft,
        random);
      int startY = Math.Max(0, column.StartY);
      for (int y = startY; y < snapshot.Metadata.Height; y++)
      {
        WorldTile tile = snapshot.GetTile(x, y);
        if (!tile.IsActive && tile.WallType == 0)
        {
          continue;
        }

        bool convertTiles = tile.IsActive && LegacyWorldInfectionPolicy.ShouldConvertTiles(tile);
        bool convertWalls = tile.WallType > 0 &&
          LegacyWorldInfectionPolicy.ShouldConvertWalls(tile);
        if (!convertTiles && !convertWalls)
        {
          continue;
        }

        LegacyWorldInfectionConversionCommand command =
          new(
            state.ReserveSequence(),
            x,
            y,
            column.ConversionType,
            convertTiles,
            convertWalls);
        if (!command.IsValid(snapshot))
        {
          throw new InvalidOperationException(
            "World infection conversion command failed its immutable input validation.");
        }

        commands.Add(command);
      }
    }
  }
}
