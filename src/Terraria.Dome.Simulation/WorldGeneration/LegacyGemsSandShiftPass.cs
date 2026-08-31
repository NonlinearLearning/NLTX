using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyGemsSandShiftPass
{
  private const int WorldInset = 10;
  private const int FirstColumn = 5;
  private const int ColumnInset = 5;

  public static void AppendCommands(
    WorldGridSnapshot snapshot,
    int undergroundDesertLeft,
    int undergroundDesertRight,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(commands);
    if (undergroundDesertLeft < 0 || undergroundDesertRight < undergroundDesertLeft ||
        undergroundDesertRight > snapshot.Metadata.Width)
    {
      throw new ArgumentOutOfRangeException(nameof(undergroundDesertLeft));
    }

    Dictionary<(int X, int Y), WorldTile> projectedTiles = new();
    for (int pass = 0; pass < 2; pass++)
    {
      int direction = pass == 0 ? 1 : -1;
      int startX = pass == 0 ? FirstColumn : snapshot.Metadata.Width - ColumnInset;
      int endX = pass == 0 ? snapshot.Metadata.Width - ColumnInset : FirstColumn;
      for (int x = startX; x != endX; x += direction)
      {
        if (x <= undergroundDesertLeft || x >= undergroundDesertRight)
        {
          for (int y = WorldInset; y < snapshot.Metadata.Height - WorldInset; y++)
          {
            WorldTile current = GetProjectedTile(snapshot, projectedTiles, x, y);
            WorldTile below = GetProjectedTile(snapshot, projectedTiles, x, y + 1);
            if (!IsSand(current) || !IsSand(below))
            {
              continue;
            }

            int targetX = x + direction;
            if (!IsInsideInset(snapshot, targetX, y) ||
                !IsInsideInset(snapshot, targetX, y + 1))
            {
              continue;
            }

            WorldTile target = GetProjectedTile(snapshot, projectedTiles, targetX, y);
            WorldTile targetBelow = GetProjectedTile(snapshot, projectedTiles, targetX, y + 1);
            if (target.IsActive || targetBelow.IsActive)
            {
              continue;
            }

            int targetY = y + 1;
            while (IsInsideInset(snapshot, targetX, targetY) &&
                   !GetProjectedTile(snapshot, projectedTiles, targetX, targetY).IsActive)
            {
              targetY++;
            }

            targetY--;
            if (targetY < y + 1)
            {
              continue;
            }

            TileChangeCommand kill = new(
              state.ReserveSequence(),
              x,
              y,
              TileChangeKind.Kill,
              TileType: 0,
              PreserveTileState: true,
              Source: "worldgen.gems.sand-shift");
            TileChangeCommand place = new(
              state.ReserveSequence(),
              targetX,
              targetY,
              TileChangeKind.UpdateTileType,
              current.Type,
              IsActive: true,
              Source: "worldgen.gems.sand-shift");
            commands.Add(kill);
            commands.Add(place);
            projectedTiles[(x, y)] = TileMutationProjection.Apply(current, kill);
            projectedTiles[(targetX, targetY)] = TileMutationProjection.Apply(
              GetProjectedTile(snapshot, projectedTiles, targetX, targetY),
              place);
          }
        }
      }
    }
  }

  private static bool IsSand(WorldTile tile)
  {
    return tile.IsActive && ConversionSandTileRegistry.RegisterDefaults().Contains(tile.Type);
  }

  private static bool IsInsideInset(WorldGridSnapshot snapshot, int x, int y)
  {
    return x >= WorldInset && x < snapshot.Metadata.Width - WorldInset &&
      y >= WorldInset && y < snapshot.Metadata.Height - WorldInset;
  }

  private static WorldTile GetProjectedTile(
    WorldGridSnapshot snapshot,
    IReadOnlyDictionary<(int X, int Y), WorldTile> projectedTiles,
    int x,
    int y)
  {
    return projectedTiles.TryGetValue((x, y), out WorldTile projected)
      ? projected
      : snapshot.GetTile(x, y);
  }
}
