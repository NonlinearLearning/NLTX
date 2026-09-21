using System;
using System.Collections.Generic;

namespace Terraria.WorldGeneration.Queries;

/// <summary>
/// Calculates the Version4 jungle conversion bounds from explicit column observations.
/// </summary>
public static class JungleRegionBoundsCalculationQuery
{
  private const int EdgePadding = 5;

  public static JungleRegionBoundsCalculationResult Calculate(
    in JungleRegionBoundsCalculationInput input)
  {
    IReadOnlyList<bool> columns = input.ColumnContainsJungleGrass;
    ArgumentNullException.ThrowIfNull(columns);
    if (columns.Count != input.MaxTilesX)
    {
      throw new ArgumentException(
        "Jungle column observations must cover the complete world width.",
        nameof(input));
    }

    int jungleMinX = 0;
    for (int columnX = EdgePadding;
         columnX < input.MaxTilesX - EdgePadding;
         columnX++)
    {
      if (columns[columnX])
      {
        jungleMinX = columnX;
        break;
      }
    }

    int jungleMaxX = 0;
    for (int columnX = input.MaxTilesX - EdgePadding;
         columnX > EdgePadding;
         columnX--)
    {
      if (columns[columnX])
      {
        jungleMaxX = columnX;
        break;
      }
    }

    return new JungleRegionBoundsCalculationResult(jungleMinX, jungleMaxX);
  }
}
