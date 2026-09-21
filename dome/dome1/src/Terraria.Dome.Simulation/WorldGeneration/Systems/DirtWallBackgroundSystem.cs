using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration.Systems;

public sealed class DirtWallBackgroundSystem
{
  private const ushort DirtWallType = 2;
  private const ushort IceWallType = 40;
  private const ushort IceTileType = 147;
  private const ushort ProtectedWallType = 64;
  private const int MaximumSurfaceOffset = 10;

  public void AppendCommands(
    WorldGridSnapshot snapshot,
    LegacyTerrainRuntimeProfile profile,
    LegacyPassRandomState random,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(profile);
    ArgumentNullException.ThrowIfNull(random);
    ArgumentNullException.ThrowIfNull(commands);
    int worldSurfaceY = LegacyMainWorldSurfacePolicy.Resolve(profile, snapshot.Metadata);
    AppendCommands(snapshot, worldSurfaceY, random, ref state, commands);
  }

  public void AppendCommands(
    WorldGridSnapshot snapshot,
    LegacyTerrainRuntimeProfile profile,
    IReadOnlyList<int> surfaceOffsetChanges,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(profile);
    ArgumentNullException.ThrowIfNull(surfaceOffsetChanges);
    ArgumentNullException.ThrowIfNull(commands);
    int worldSurfaceY = LegacyMainWorldSurfacePolicy.Resolve(profile, snapshot.Metadata);
    AppendCommands(
      snapshot,
      worldSurfaceY,
      surfaceOffsetChanges,
      ref state,
      commands);
  }

  public void AppendCommands(
    WorldGridSnapshot snapshot,
    int worldSurfaceY,
    LegacyPassRandomState random,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(random);
    ArgumentNullException.ThrowIfNull(commands);
    List<int> surfaceOffsetChanges = new(snapshot.Metadata.Width - 2);
    for (int x = 1; x < snapshot.Metadata.Width - 1; x++)
    {
      surfaceOffsetChanges.Add(random.Next(-1, 2));
    }

    AppendCommands(
      snapshot,
      worldSurfaceY,
      surfaceOffsetChanges,
      ref state,
      commands);
  }

  public void AppendCommands(
    WorldGridSnapshot snapshot,
    int worldSurfaceY,
    IReadOnlyList<int> surfaceOffsetChanges,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(surfaceOffsetChanges);
    ArgumentNullException.ThrowIfNull(commands);
    if (worldSurfaceY < 0 || worldSurfaceY >= snapshot.Metadata.Height)
    {
      throw new ArgumentOutOfRangeException(nameof(worldSurfaceY));
    }

    if (surfaceOffsetChanges.Count != snapshot.Metadata.Width - 2)
    {
      throw new ArgumentException(
        "A surface offset change is required for every non-edge column.",
        nameof(surfaceOffsetChanges));
    }

    if (state.Stage < WorldGenerationStage.Terrain)
    {
      throw new InvalidOperationException(
        "Dirt wall backgrounds require the terrain stage snapshot.");
    }

    int surfaceOffset = 0;
    for (int x = 1; x < snapshot.Metadata.Width - 1; x++)
    {
      int offsetChange = surfaceOffsetChanges[x - 1];
      if (offsetChange < -1 || offsetChange > 1)
      {
        throw new ArgumentOutOfRangeException(nameof(surfaceOffsetChanges));
      }

      surfaceOffset = Math.Clamp(surfaceOffset + offsetChange, 0, MaximumSurfaceOffset);
      bool hasEnclosedTile = false;
      ushort wallType = DirtWallType;
      int endYExclusive = Math.Min(
        snapshot.Metadata.Height,
        worldSurfaceY + MaximumSurfaceOffset);
      for (int y = 0; y < endYExclusive && y <= worldSurfaceY + surfaceOffset; y++)
      {
        WorldTile tile = snapshot.GetTile(x, y);
        if (tile.IsActive)
        {
          wallType = tile.Type == IceTileType ? IceWallType : DirtWallType;
        }

        if (hasEnclosedTile && tile.WallType != ProtectedWallType)
        {
          commands.Add(new TileChangeCommand(
            state.ReserveSequence(),
            x,
            y,
            TileChangeKind.SetWall,
            0,
            wallType,
            Source: "worldgen.dirt-wall"));
        }

        if (IsEnclosed(snapshot, x, y))
        {
          hasEnclosedTile = true;
        }
      }
    }
  }

  private static bool IsEnclosed(WorldGridSnapshot snapshot, int x, int y)
  {
    return snapshot.GetTile(x, y).IsActive &&
      snapshot.GetTile(x - 1, y).IsActive &&
      snapshot.GetTile(x + 1, y).IsActive &&
      snapshot.GetTile(x, y + 1).IsActive &&
      snapshot.GetTile(x - 1, y + 1).IsActive &&
      snapshot.GetTile(x + 1, y + 1).IsActive;
  }
}
