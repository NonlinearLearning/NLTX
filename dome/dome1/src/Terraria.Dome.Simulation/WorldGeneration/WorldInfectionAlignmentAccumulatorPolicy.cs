using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class WorldInfectionAlignmentAccumulatorPolicy
{
  public static WorldInfectionAlignmentAccumulatorResult AddColumn(
    WorldInfectionAlignmentAccumulator state,
    WorldInfectionAlignmentSnapshot column,
    bool publishBeforeColumn)
  {
    if (state.TotalEvil < 0 || state.TotalBlood < 0 || state.TotalGood < 0 ||
        state.TotalSolid < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(state));
    }

    WorldInfectionAlignmentSnapshot published = publishBeforeColumn
      ? new WorldInfectionAlignmentSnapshot(
        state.TotalEvil,
        state.TotalBlood,
        state.TotalGood,
        state.TotalSolid)
      : default;
    WorldInfectionAlignmentAccumulator baseState = publishBeforeColumn
      ? WorldInfectionAlignmentAccumulator.Empty
      : state;
    WorldInfectionAlignmentAccumulator nextState = new(
      checked(baseState.TotalEvil + column.TotalEvil),
      checked(baseState.TotalBlood + column.TotalBlood),
      checked(baseState.TotalGood + column.TotalGood),
      checked(baseState.TotalSolid + column.TotalSolid));
    return new WorldInfectionAlignmentAccumulatorResult(nextState, publishBeforeColumn, published);
  }
}
