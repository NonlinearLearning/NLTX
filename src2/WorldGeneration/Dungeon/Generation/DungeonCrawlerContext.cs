namespace Terraria.WorldGeneration.Dungeon.Generation;

public sealed class DungeonCrawlerContext
{
  public DungeonGenerationContextComponent? CurrentDungeonData { get; private set; }

  public DungeonGenerationCollectionsComponent Collections { get; } = new();

  public DungeonGenerationScalarStateComponent Scalars { get; } = new();

  public DungeonLegacyPlacementStateComponent LegacyPlacement { get; } = new();

  public DungeonLegacyRuleStateComponent LegacyRules { get; } = new();

  public DungeonGenerationContextComponent Begin(
    int type,
    int iteration)
  {
    if (CurrentDungeonData is not null)
    {
      throw new InvalidOperationException(
        "A dungeon iteration is already active for this crawler context.");
    }

    CurrentDungeonData = new DungeonGenerationContextComponent(type, iteration);
    return CurrentDungeonData;
  }

  public DungeonGenerationContextComponent RequireCurrent()
  {
    return CurrentDungeonData ?? throw new InvalidOperationException(
      "No dungeon iteration is active for this crawler context.");
  }

  public void End()
  {
    if (CurrentDungeonData is null)
    {
      return;
    }

    Collections.ClearTransientWork();
    LegacyRules.EndGeneration();
    CurrentDungeonData = null;
  }
}
