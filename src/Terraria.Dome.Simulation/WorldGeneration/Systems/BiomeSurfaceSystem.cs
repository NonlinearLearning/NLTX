using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration.Systems;

public sealed class BiomeSurfaceSystem
{
  public BiomeSurfaceResult AppendCommands(
    WorldGridSnapshot snapshot,
    BiomeSurfaceComponent profile,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(commands);
    if (profile.BiomeId != "default" && profile.Definition is null)
    {
      return BiomeSurfaceResult.Unsupported($"Unsupported biome profile: {profile.BiomeId}");
    }

    if (state.Stage < WorldGenerationStage.Biome &&
        !state.TryAdvance(WorldGenerationStage.Biome))
    {
      return BiomeSurfaceResult.Unsupported("Biome stage could not be started.");
    }

    if (profile.Definition is null)
    {
      return BiomeSurfaceResult.Success();
    }

    BiomeSurfaceDefinition definition = profile.Definition.Value;
    int startY = Math.Clamp(profile.SurfaceY, 0, snapshot.Metadata.Height - 1);
    int endY = Math.Min(snapshot.Metadata.Height, startY + definition.Depth);
    for (int x = 0; x < snapshot.Metadata.Width; x++)
    {
      for (int y = startY; y < endY; y++)
      {
        WorldTile current = snapshot.GetTile(x, y);
        if (!current.IsActive)
        {
          continue;
        }

        commands.Add(new TileChangeCommand(
          state.ReserveSequence(),
          x,
          y,
          TileChangeKind.UpdateTileType,
          definition.SurfaceTileType,
          WallType: definition.SurfaceWallType,
          FrameX: current.FrameX,
          FrameY: current.FrameY,
          Source: definition.Id,
          Priority: 20));
        commands.Add(new TileChangeCommand(
          state.ReserveSequence(),
          x,
          y,
          TileChangeKind.SetWall,
          0,
          WallType: definition.SurfaceWallType,
          Source: definition.Id,
          Priority: 20));
      }
    }

    return BiomeSurfaceResult.Success();
  }
}

public sealed record BiomeSurfaceResult(bool Supported, string? FailureReason)
{
  public static BiomeSurfaceResult Success()
  {
    return new BiomeSurfaceResult(true, null);
  }

  public static BiomeSurfaceResult Unsupported(string reason)
  {
    return new BiomeSurfaceResult(false, reason);
  }
}
