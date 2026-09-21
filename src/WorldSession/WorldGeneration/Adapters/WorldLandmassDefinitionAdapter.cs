using System;
using System.Collections.Generic;
using Terraria.WorldGeneration.Components;
using Terraria.WorldGeneration.Definitions;

namespace Terraria.WorldGeneration.Adapters;

/// <summary>
/// Converts current-project landmass definitions into copied generation values.
/// </summary>
public static class WorldLandmassDefinitionAdapter
{
  public static IReadOnlyList<WorldGenerationLandmassValue> ToValues(
    IReadOnlyList<WorldLandmassDefinition> definitions)
  {
    ArgumentNullException.ThrowIfNull(definitions);
    if (definitions.Count == 0)
    {
      return Array.Empty<WorldGenerationLandmassValue>();
    }

    WorldGenerationLandmassValue[] values =
      new WorldGenerationLandmassValue[definitions.Count];
    for (int index = 0; index < definitions.Count; index++)
    {
      WorldLandmassDefinition definition = definitions[index];
      values[index] = new WorldGenerationLandmassValue(
        (int)definition.DataType,
        definition.Position.X,
        definition.Position.Y,
        definition.RadiusOrHalfSize,
        definition.Style);
    }

    return Array.AsReadOnly(values);
  }
}
