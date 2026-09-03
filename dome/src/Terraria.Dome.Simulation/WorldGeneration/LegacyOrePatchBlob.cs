using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyOrePatchBlob
{
  public static bool TryAppendCommands(
    WorldGridSnapshot snapshot,
    int centerX,
    int centerY,
    ushort tileType,
    LegacyPassRandomState random,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(random);
    ArgumentNullException.ThrowIfNull(commands);
    Dictionary<(int X, int Y), WorldTile> tiles = new();
    List<LegacyOrePatchBlobMutation> mutations = new();
    bool isInBounds = true;
    double currentX = centerX;
    double currentY = centerY;
    double driftX = random.NextDouble() * 0.6 - 0.3;
    double driftY = random.NextDouble() * 0.5 + 0.5;
    double radius = random.Next(5, 9);
    int steps = random.Next(9, 14);
    if (random.Next(3) == 0)
    {
      radius += random.Next(2);
    }

    if (random.Next(3) == 0)
    {
      steps += random.Next(2);
    }

    while (steps > 0)
    {
      steps--;
      int range = (int)radius * 4;
      for (int x = (int)currentX - range; x <= currentX + range; x++)
      {
        for (int y = (int)currentY - range; y <= currentY + range; y++)
        {
          double innerRadius = radius * (0.5 + random.NextDouble() * 0.5) * 0.1;
          double outerRadius = radius * (0.7 + random.NextDouble() * 0.6) * 0.3;
          if (random.Next(8) == 0)
          {
            outerRadius *= 2.0;
          }

          double distance = Math.Sqrt(
            (currentX - x) * (currentX - x) + (currentY - y) * (currentY - y));
          if (distance >= outerRadius)
          {
            continue;
          }

          if (!snapshot.Metadata.IsInside(x, y))
          {
            isInBounds = false;
            continue;
          }

          WorldTile tile = GetTile(snapshot, tiles, x, y);
          if (distance < innerRadius)
          {
            AddMutation(tiles, mutations, x, y, tile with { IsActive = false });
            continue;
          }

          bool isActive = random.Next(4) == 0 || tile.IsActive;
          AddMutation(tiles, mutations, x, y, tile with { Type = tileType, IsActive = isActive });
          AppendOreHelperMutations(snapshot, tiles, mutations, x, y, ref isInBounds);
        }
      }

      currentX += driftX;
      currentY += driftY;
      driftX = Math.Clamp(driftX + random.NextDouble() * 0.2 - 0.1, -0.3, 0.3);
      driftY = Math.Clamp(driftY + random.NextDouble() * 0.2 - 0.1, 0.5, 1.0);
    }

    if (!isInBounds)
    {
      return false;
    }

    foreach (LegacyOrePatchBlobMutation mutation in mutations)
    {
      commands.Add(new TileChangeCommand(
        state.ReserveSequence(),
        mutation.X,
        mutation.Y,
        TileChangeKind.UpdateTileType,
        mutation.Tile.Type,
        IsActive: mutation.Tile.IsActive,
        Source: "worldgen.ore.OrePatch.blob"));
    }

    return true;
  }

  private static void AddMutation(
    Dictionary<(int X, int Y), WorldTile> tiles,
    List<LegacyOrePatchBlobMutation> mutations,
    int x,
    int y,
    WorldTile tile)
  {
    tiles[(x, y)] = tile;
    mutations.Add(new LegacyOrePatchBlobMutation(x, y, tile));
  }

  private static void AppendOreHelperMutations(
    WorldGridSnapshot snapshot,
    Dictionary<(int X, int Y), WorldTile> tiles,
    List<LegacyOrePatchBlobMutation> mutations,
    int centerX,
    int centerY,
    ref bool isInBounds)
  {
    for (int x = centerX - 1; x <= centerX + 1; x++)
    {
      for (int y = centerY - 1; y <= centerY + 1; y++)
      {
        if (!snapshot.Metadata.IsInside(x, y))
        {
          isInBounds = false;
          continue;
        }

        WorldTile tile = GetTile(snapshot, tiles, x, y);
        if (tile.Type is 1 or 40)
        {
          AddMutation(tiles, mutations, x, y, tile with { Type = 0 });
        }
      }
    }
  }

  private static WorldTile GetTile(
    WorldGridSnapshot snapshot,
    IReadOnlyDictionary<(int X, int Y), WorldTile> tiles,
    int x,
    int y)
  {
    return tiles.TryGetValue((x, y), out WorldTile tile) ? tile : snapshot.GetTile(x, y);
  }
}

public readonly record struct LegacyOrePatchBlobMutation(int X, int Y, WorldTile Tile);
