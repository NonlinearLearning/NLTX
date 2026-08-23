using System;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed record LegacyTileRunnerSnapshotExecutionContext(
  int WaterLine,
  int LavaLine,
  double WorldSurface,
  short LiquidType,
  bool RemixWorld,
  int RockLayer,
  int MaxTilesY,
  bool IsOceanDepth,
  int SourceLine);

public static class LegacyTileRunnerSnapshotExecution
{
  public static LegacyTileRunnerInvocationProvenance Execute(
    WorldGridSnapshot snapshot,
    LegacyTileRunnerPassInvocation invocation,
    LegacyTileRunnerSnapshotExecutionContext context,
    long tileSequence,
    long liquidSequence,
    int invocationIndex)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(invocation);
    ArgumentNullException.ThrowIfNull(context);
    if (!snapshot.Metadata.IsInside(invocation.Request.X, invocation.Request.Y))
    {
      throw new ArgumentOutOfRangeException(nameof(invocation));
    }

    WorldTile tile = snapshot.GetTile(invocation.Request.X, invocation.Request.Y);
    LegacyTileRunnerCommandBatch commands = LegacyTileRunnerCommandEmitter.Create(
      invocation.Request.X,
      invocation.Request.Y,
      tileSequence,
      liquidSequence,
      invocation.Request.TileType,
      invocation.Request.AddTile,
      invocation.Request.NoYChange,
      tile.IsActive,
      context.WaterLine,
      context.LavaLine,
      context.WorldSurface,
      context.LiquidType,
      context.RemixWorld,
      context.RockLayer,
      context.MaxTilesY,
      context.IsOceanDepth);
    return LegacyTileRunnerInvocationProvenanceFactory.Create(
      invocation,
      commands,
      invocationIndex,
      context.SourceLine);
  }
}
