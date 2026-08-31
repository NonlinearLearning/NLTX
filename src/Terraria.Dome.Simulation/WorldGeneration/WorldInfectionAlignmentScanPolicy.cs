using System;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class WorldInfectionAlignmentScanPolicy
{
  public static WorldInfectionAlignmentScanResult Advance(
    WorldInfectionAlignmentScanState state,
    WorldGridSnapshot snapshot,
    int surfaceY)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    TileCountSchedulingResult scheduling = TileCountSchedulingPolicy.Advance(
      state.Scheduling,
      snapshot.Metadata.Width);
    if (!scheduling.ShouldScanColumn)
    {
      return new WorldInfectionAlignmentScanResult(
        new WorldInfectionAlignmentScanState(scheduling.State, state.Accumulator),
        Scanned: false,
        ColumnX: scheduling.ColumnX,
        Column: default,
        HasPublished: false,
        Published: default);
    }

    WorldInfectionAlignmentSnapshot column = WorldInfectionAlignmentScanQuery.ScanColumn(
      snapshot,
      scheduling.ColumnX,
      surfaceY);
    WorldInfectionAlignmentAccumulatorResult accumulation =
      WorldInfectionAlignmentAccumulatorPolicy.AddColumn(
        state.Accumulator,
        column,
        publishBeforeColumn: scheduling.ColumnX == 0);
    return new WorldInfectionAlignmentScanResult(
      new WorldInfectionAlignmentScanState(scheduling.State, accumulation.State),
      Scanned: true,
      ColumnX: scheduling.ColumnX,
      Column: column,
      accumulation.HasPublished,
      accumulation.Published);
  }
}
