namespace Terraria.NonAuthoritative.ContentDefinitions;

public enum AnchorKind
{
  None,
  Top,
  Bottom,
  Left,
  Right
}

public enum LiquidKind
{
  None,
  Water,
  Lava
}

public enum LiquidPlacementMode
{
  None,
  Allowed,
  WaterOnly,
  LavaOnly,
  WaterAndLava
}

public readonly record struct TileAnchor
{
  public TileAnchor(AnchorKind kind, int width)
  {
    if (width < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(width));
    }

    Kind = kind;
    Width = width;
  }

  public AnchorKind Kind { get; }

  public int Width { get; }
}

public readonly record struct TileReadSnapshot(
  int TileType,
  int WallType,
  bool HasSolidAnchor,
  bool HasWallAnchor,
  LiquidKind Liquid);

public sealed class TileObjectPlacementDefinition
{
  public TileObjectPlacementDefinition(int tileType)
  {
    if (tileType < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(tileType));
    }

    TileType = tileType;
  }

  public int TileType { get; }

  public bool UsesCustomCanPlace { get; set; }

  public bool UsesGlobalLiquidChecks { get; set; }

  public TileAnchor AnchorTop { get; set; }

  public TileAnchor AnchorBottom { get; set; }

  public TileAnchor AnchorLeft { get; set; }

  public TileAnchor AnchorRight { get; set; }

  public bool AnchorWall { get; set; }

  public IReadOnlyList<int> AnchorValidTiles { get; set; } = Array.Empty<int>();

  public IReadOnlyList<int> AnchorInvalidTiles { get; set; } = Array.Empty<int>();

  public IReadOnlyList<int> AnchorAlternateTiles { get; set; } = Array.Empty<int>();

  public IReadOnlyList<int> AnchorValidWalls { get; set; } = Array.Empty<int>();

  public bool WaterDeath { get; set; }

  public bool LavaDeath { get; set; }

  public LiquidPlacementMode WaterPlacement { get; set; }

  public LiquidPlacementMode LavaPlacement { get; set; }
}

public readonly record struct PlacementEligibilityResult(
  bool IsAllowed,
  string? FailureReason);

public static class TilePlacementRuleQuery
{
  public static PlacementEligibilityResult Evaluate(
    TileObjectPlacementDefinition definition,
    TileReadSnapshot snapshot)
  {
    ArgumentNullException.ThrowIfNull(definition);
    if (definition.AnchorInvalidTiles.Contains(snapshot.TileType))
    {
      return new PlacementEligibilityResult(false, "invalid-tile");
    }

    bool validTile = definition.AnchorValidTiles.Count == 0 ||
      definition.AnchorValidTiles.Contains(snapshot.TileType) ||
      definition.AnchorAlternateTiles.Contains(snapshot.TileType);
    bool validAnchor = snapshot.HasSolidAnchor && validTile;
    if (definition.AnchorWall)
    {
      validAnchor |= snapshot.HasWallAnchor &&
        (definition.AnchorValidWalls.Count == 0 || definition.AnchorValidWalls.Contains(snapshot.WallType));
    }

    if (!validAnchor && !definition.UsesCustomCanPlace)
    {
      return new PlacementEligibilityResult(false, "anchor");
    }

    if (!AllowsLiquid(definition, snapshot.Liquid))
    {
      return new PlacementEligibilityResult(false, "liquid");
    }

    return new PlacementEligibilityResult(true, null);
  }

  private static bool AllowsLiquid(
    TileObjectPlacementDefinition definition,
    LiquidKind liquid)
  {
    return liquid switch
    {
      LiquidKind.None => true,
      LiquidKind.Water => !definition.WaterDeath &&
        AllowsMode(definition.WaterPlacement, LiquidKind.Water),
      LiquidKind.Lava => !definition.LavaDeath &&
        AllowsMode(definition.LavaPlacement, LiquidKind.Lava),
      _ => false
    };
  }

  private static bool AllowsMode(LiquidPlacementMode mode, LiquidKind liquid)
  {
    return mode switch
    {
      LiquidPlacementMode.Allowed => true,
      LiquidPlacementMode.WaterOnly => liquid == LiquidKind.Water,
      LiquidPlacementMode.LavaOnly => liquid == LiquidKind.Lava,
      LiquidPlacementMode.WaterAndLava => true,
      _ => false
    };
  }
}

public static class TilePlacementDefinitionRegistrationSystem
{
  public static TileObjectPlacementDefinition Create(int tileType)
  {
    return new TileObjectPlacementDefinition(tileType);
  }
}
