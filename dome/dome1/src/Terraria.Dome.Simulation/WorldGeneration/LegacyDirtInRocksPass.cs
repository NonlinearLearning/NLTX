using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyDirtInRocksPass
{
  public const int SourceLine = 12321;

  public static void AppendCommands(
    WorldGridSnapshot snapshot,
    LegacyTerrainRuntimeProfile profile,
    LegacyPassRandomState random,
    bool isRemixWorld,
    bool isSkyblockWorld,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(profile);
    ArgumentNullException.ThrowIfNull(random);
    ArgumentNullException.ThrowIfNull(commands);
    profile.Validate(snapshot.Metadata);
    LegacyDirtInRocksPassDefinition.Validate();
    if (isSkyblockWorld)
    {
      return;
    }

    if (state.Stage < WorldGenerationStage.Cave &&
        !state.TryAdvance(WorldGenerationStage.Cave))
    {
      throw new InvalidOperationException("DirtInRocks could not enter the cave stage.");
    }

    LegacyTileRunnerPassInput recipe =
      LegacyDirtInRocksPassDefinition.CreateDefaultRecipe();
    int minimumY = checked((int)profile.RockLayerLow);
    if (minimumY < 0 || minimumY >= snapshot.Metadata.Height)
    {
      throw new ArgumentOutOfRangeException(nameof(profile));
    }

    int worldSurfaceY = LegacyMainWorldSurfacePolicy.Resolve(profile, snapshot.Metadata);
    int rockLayerY = checked((int)profile.RockLayer);
    int invocationCount = LegacyDirtInRocksPassDefinition.CalculateInvocationCount(
      snapshot.Metadata.Width,
      snapshot.Metadata.Height);
    Dictionary<(int X, int Y), WorldTile> projectedTiles = new();
    for (int index = 0; index < invocationCount; index++)
    {
      LegacyTileRunnerPassInvocation invocation =
        LegacyTileRunnerPassInvocationFactory.Create(
          recipe,
          random,
          minimumXInclusive: 0,
          maximumXExclusive: snapshot.Metadata.Width,
          minimumYInclusive: minimumY,
          maximumYExclusive: snapshot.Metadata.Height);
      LegacyTileRunnerTraversal.AppendCommands(
        snapshot,
        invocation,
        random,
        worldSurfaceY,
        rockLayerY,
        ref state,
        commands,
        projectedTiles: projectedTiles);
    }

    if (isRemixWorld)
    {
      AppendRemixCommands(
        snapshot,
        profile,
        random,
        ref state,
        commands,
        projectedTiles);
    }
  }

  private static void AppendRemixCommands(
    WorldGridSnapshot snapshot,
    LegacyTerrainRuntimeProfile profile,
    LegacyPassRandomState random,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands,
    IDictionary<(int X, int Y), WorldTile> projectedTiles)
  {
    int worldSurfaceY = LegacyMainWorldSurfacePolicy.Resolve(profile, snapshot.Metadata);
    for (int x = 0; x < snapshot.Metadata.Width; x++)
    {
      int startY = checked(
        worldSurfaceY + random.Next(
          LegacyDirtInRocksPassDefinition.MinimumRemixOffset,
          LegacyDirtInRocksPassDefinition.MaximumRemixOffsetExclusive));
      for (int y = Math.Max(0, startY); y < snapshot.Metadata.Height; y++)
      {
        (int X, int Y) coordinates = (x, y);
        WorldTile tile = projectedTiles.TryGetValue(coordinates, out WorldTile projectedTile)
          ? projectedTile
          : snapshot.GetTile(x, y);
        int targetType = tile.IsActive switch
        {
          true when tile.Type == 0 => 1,
          true when tile.Type == 1 => 0,
          _ => -1
        };
        if (targetType < 0)
        {
          continue;
        }

        TileChangeCommand command = new(
          state.ReserveSequence(),
          x,
          y,
          TileChangeKind.UpdateTileType,
          checked((ushort)targetType),
          Source: "worldgen.cave.DirtInRocks.remix",
          IsActive: true);
        commands.Add(command);
        projectedTiles[coordinates] = TileMutationProjection.Apply(tile, command);
      }
    }
  }
}
