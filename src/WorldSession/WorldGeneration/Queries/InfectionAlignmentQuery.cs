using System;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Queries;

public static class InfectionAlignmentQuery
{
  public static InfectionAlignmentSnapshot Snapshot(
    InfectionAlignmentComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    return component.CreateSnapshot();
  }
}
