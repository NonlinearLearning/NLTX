using System;
using Terraria.WorldGeneration.Components;
using Terraria.WorldGeneration.Queries;

static class Program
{
  private static int Main()
  {
    try
    {
      VerifyInclusiveJungleConversionRange();
      VerifyInvertedRangeRemainsEmpty();
      VerifyVersion4BoundaryScanRanges();
      VerifyNoMatchingColumnReturnsZeroBounds();
      VerifyColumnObservationsAreCopied();
      Console.WriteLine("C07 jungle-region focused verifier passed.");
      return 0;
    }
    catch (Exception exception)
    {
      Console.Error.WriteLine(exception);
      return 1;
    }
  }

  private static void VerifyInclusiveJungleConversionRange()
  {
    JungleRegionStructureSnapshot snapshot = new(
      GenerationId: 501,
      ExtraBastStatueCount: 0,
      ExtraBastStatueCountMax: 2,
      JungleOriginX: 30,
      JungleMinX: 10,
      JungleMaxX: 20,
      JungleHut: 119,
      MudWall: false);

    Require(
      JungleRegionStructureQuery.IsWithinJungleConversionRange(
        snapshot,
        10),
      "jungle conversion range includes its minimum endpoint");
    Require(
      JungleRegionStructureQuery.IsWithinJungleConversionRange(
        snapshot,
        20),
      "jungle conversion range includes its maximum endpoint");
    Require(
      !JungleRegionStructureQuery.IsWithinJungleConversionRange(
        snapshot,
        9),
      "jungle conversion range excludes the column before its minimum");
    Require(
      !JungleRegionStructureQuery.IsWithinJungleConversionRange(
        snapshot,
        21),
      "jungle conversion range excludes the column after its maximum");
  }

  private static void VerifyInvertedRangeRemainsEmpty()
  {
    JungleRegionStructureSnapshot snapshot = new(
      GenerationId: 502,
      ExtraBastStatueCount: 0,
      ExtraBastStatueCountMax: 0,
      JungleOriginX: 0,
      JungleMinX: 20,
      JungleMaxX: 10,
      JungleHut: 0,
      MudWall: false);

    Require(
      !JungleRegionStructureQuery.IsWithinJungleConversionRange(
        snapshot,
        15),
      "inverted jungle bounds must not be normalized by a pure query");
  }

  private static void VerifyVersion4BoundaryScanRanges()
  {
    bool[] columns = new bool[20];
    columns[4] = true;
    columns[5] = true;
    columns[14] = true;
    columns[15] = true;
    columns[16] = true;
    columns[19] = true;

    JungleRegionBoundsCalculationResult result =
      JungleRegionBoundsCalculationQuery.Calculate(
        new JungleRegionBoundsCalculationInput(20, columns));

    Require(result.JungleMinX == 5, "left scan starts at column five");
    Require(
      result.JungleMaxX == 15,
      "right scan starts at maxTilesX minus five and stops after the first hit");
  }

  private static void VerifyNoMatchingColumnReturnsZeroBounds()
  {
    JungleRegionBoundsCalculationResult result =
      JungleRegionBoundsCalculationQuery.Calculate(
        new JungleRegionBoundsCalculationInput(20, new bool[20]));

    Require(result.JungleMinX == 0, "no left match preserves the sourced zero default");
    Require(result.JungleMaxX == 0, "no right match preserves the sourced zero default");
  }

  private static void VerifyColumnObservationsAreCopied()
  {
    bool[] columns = new bool[12];
    columns[5] = true;
    JungleRegionBoundsCalculationInput input =
      new(12, columns);
    columns[5] = false;

    JungleRegionBoundsCalculationResult result =
      JungleRegionBoundsCalculationQuery.Calculate(in input);

    Require(result.JungleMinX == 5, "boundary input does not retain a mutable source array");
    Require(result.JungleMaxX == 0, "copied boundary input preserves the right scan range");
  }

  private static void Require(bool condition, string message)
  {
    if (!condition)
    {
      throw new InvalidOperationException(message);
    }
  }
}
