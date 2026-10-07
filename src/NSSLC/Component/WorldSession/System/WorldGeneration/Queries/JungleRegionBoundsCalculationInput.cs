using System;
using System.Collections.Generic;

namespace Terraria.WorldGeneration.Queries;

/// <summary>
/// Contains one generation-scoped column observation for the jungle-boundary scan.
/// </summary>
public readonly record struct JungleRegionBoundsCalculationInput
{
  public JungleRegionBoundsCalculationInput(
    int maxTilesX,
    IReadOnlyList<bool> columnContainsJungleGrass)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(maxTilesX);
    ArgumentNullException.ThrowIfNull(columnContainsJungleGrass);
    if (columnContainsJungleGrass.Count != maxTilesX)
    {
      throw new ArgumentException(
        "Jungle column observations must cover the complete world width.",
        nameof(columnContainsJungleGrass));
    }

    bool[] copy = new bool[maxTilesX];
    for (int columnX = 0; columnX < maxTilesX; columnX++)
    {
      copy[columnX] = columnContainsJungleGrass[columnX];
    }

    MaxTilesX = maxTilesX;
    ColumnContainsJungleGrass = Array.AsReadOnly(copy);
  }

  public int MaxTilesX { get; }

  /// <summary>
  /// Reports whether the external tile scan found an active type-60 tile in the
  /// Version4 jungle-boundary vertical range for each column.
  /// </summary>
  public IReadOnlyList<bool> ColumnContainsJungleGrass { get; }
}
