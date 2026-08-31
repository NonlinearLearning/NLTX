using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyOrePatchTrail
{
  public static bool TryAppendCommands(
    WorldGridSnapshot snapshot,
    int originX,
    int groundY,
    ushort tileType,
    LegacyPassRandomState random,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    return TryAppendCommands(
      snapshot, originX, groundY, tileType, random, ref state, commands, out _);
  }

  public static bool TryAppendCommands(
    WorldGridSnapshot snapshot,
    int originX,
    int groundY,
    ushort tileType,
    LegacyPassRandomState random,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands,
    out LegacyOrePatchTrailEnd end)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(random);
    ArgumentNullException.ThrowIfNull(commands);
    LegacyOrePatchTrailLayout layout = CreatePlacements(originX, groundY, random);
    List<(int X, int Y)> placements = layout.Placements;
    HashSet<(int X, int Y)> placementSet = new(placements);
    foreach ((int x, int y) in placements)
    {
      if (x <= 0 || y <= 0 || x >= snapshot.Metadata.Width - 1 ||
          y >= snapshot.Metadata.Height - 1)
      {
        end = default;
        return false;
      }
    }

    foreach ((int x, int y) in placements)
    {
      LegacyOreHelper.AppendCommandsExcluding(
        snapshot, x, y, placementSet, ref state, commands);
    }

    foreach ((int x, int y) in placements)
    {
      commands.Add(new TileChangeCommand(
        state.ReserveSequence(),
        x,
        y,
        TileChangeKind.UpdateTileType,
        tileType,
        Source: "worldgen.ore.OrePatch.trail"));
    }

    end = new LegacyOrePatchTrailEnd(layout.MainEndX, layout.MainEndY);
    return true;
  }

  private static LegacyOrePatchTrailLayout CreatePlacements(
    int originX,
    int groundY,
    LegacyPassRandomState random)
  {
    List<(int X, int Y)> placements = new();
    int x = originX;
    int y = groundY + random.Next(2);
    placements.Add((x, y));
    int initialY = y;
    while (y < initialY + random.Next(8, 13))
    {
      x += random.Next(-1, 2);
      y += random.Next(1, 3);
      if (random.Next(3) == 0)
      {
        y++;
      }

      placements.Add((x, y));
      if (random.Next(4) == 0)
      {
        placements.Add((x + random.Next(-2, 3), y + random.Next(2)));
      }
    }

    return new LegacyOrePatchTrailLayout(placements, x, y);
  }
}

public readonly record struct LegacyOrePatchTrailEnd(int X, int Y);

internal readonly record struct LegacyOrePatchTrailLayout(
  List<(int X, int Y)> Placements,
  int MainEndX,
  int MainEndY);
