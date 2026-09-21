using System;
using Terraria.WorldGeneration.Definitions;
using Terraria.WorldGeneration.Queries;

static class Program
{
  private static int Main()
  {
    try
    {
      VerifySourcedJungleHutWallMappings();
      VerifyUnknownJungleHutPreservesTileAndDefaultWall();
      VerifySourcedJungleHutSelection();
      VerifySelectionRollBounds();
      Console.WriteLine("C07 jungle-hut material focused verifier passed.");
      return 0;
    }
    catch (Exception exception)
    {
      Console.Error.WriteLine(exception);
      return 1;
    }
  }

  private static void VerifySourcedJungleHutWallMappings()
  {
    RequireMapping(119, 23);
    RequireMapping(120, 24);
    RequireMapping(158, 42);
    RequireMapping(175, 45);
    RequireMapping(45, 10);
  }

  private static void VerifyUnknownJungleHutPreservesTileAndDefaultWall()
  {
    const ushort unknownTileType = 321;
    var definition = JungleHutMaterialDefinitionQuery.Resolve(unknownTileType);
    Require(
      definition.HutTileType == unknownTileType,
      "unknown jungle-hut tile type must be preserved");
    Require(
      definition.WallType == 0,
      "unknown jungle-hut tile type must preserve the sourced default wall");
  }

  private static void VerifySourcedJungleHutSelection()
  {
    ushort[] expectedTileTypes = [119, 120, 158, 175, 45];
    ushort[] expectedWallTypes = [23, 24, 42, 45, 10];

    for (int roll = 0; roll < expectedTileTypes.Length; roll++)
    {
      JungleHutMaterialDefinition definition =
        JungleHutMaterialSelectionQuery.Select(
          new JungleHutMaterialSelectionRandomInput(roll));
      Require(
        definition.HutTileType == expectedTileTypes[roll],
        $"jungle-hut roll {roll} must select its sourced tile type");
      Require(
        definition.WallType == expectedWallTypes[roll],
        $"jungle-hut roll {roll} must select its sourced wall type");
    }
  }

  private static void VerifySelectionRollBounds()
  {
    RequireThrows(
      () => JungleHutMaterialSelectionQuery.Select(
        new JungleHutMaterialSelectionRandomInput(-1)),
      "negative jungle-hut selection roll");
    RequireThrows(
      () => JungleHutMaterialSelectionQuery.Select(
        new JungleHutMaterialSelectionRandomInput(5)),
      "jungle-hut selection roll upper bound");
  }

  private static void RequireMapping(ushort hutTileType, ushort wallType)
  {
    var definition = JungleHutMaterialDefinitionQuery.Resolve(hutTileType);
    Require(
      definition.HutTileType == hutTileType,
      $"jungle-hut tile type {hutTileType} must be preserved");
    Require(
      definition.WallType == wallType,
      $"jungle-hut tile type {hutTileType} must map to wall type {wallType}");
  }

  private static void Require(bool condition, string message)
  {
    if (!condition)
    {
      throw new InvalidOperationException(message);
    }
  }

  private static void RequireThrows(Action action, string message)
  {
    try
    {
      action();
    }
    catch (ArgumentOutOfRangeException)
    {
      return;
    }

    throw new InvalidOperationException(message);
  }
}
