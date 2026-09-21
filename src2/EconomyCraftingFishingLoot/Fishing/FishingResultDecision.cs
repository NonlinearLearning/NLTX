namespace NLTX.EconomyCraftingFishingLoot.Fishing;

public sealed class FishingResultDecision
{
  private FishingResultDecision(
    int rolledItemDrop,
    bool hasItemDrop,
    int rolledEnemySpawn,
    bool hasEnemySpawn,
    int rulesEvaluated,
    int randomRollCount,
    bool stoppedByRule)
  {
    RolledItemDrop = rolledItemDrop;
    HasItemDrop = hasItemDrop;
    RolledEnemySpawn = rolledEnemySpawn;
    HasEnemySpawn = hasEnemySpawn;
    RulesEvaluated = rulesEvaluated;
    RandomRollCount = randomRollCount;
    StoppedByRule = stoppedByRule;
  }

  public int RolledItemDrop { get; }

  public bool HasItemDrop { get; }

  public int RolledEnemySpawn { get; }

  public bool HasEnemySpawn { get; }

  public int RulesEvaluated { get; }

  public int RandomRollCount { get; }

  public bool StoppedByRule { get; }

  public bool IsDecisionOnly => true;

  public static FishingResultDecision ItemDrop(
    int itemTypeId,
    int enemyTypeId,
    int rulesEvaluated,
    int randomRollCount,
    bool stoppedByRule = false)
  {
    ValidateResultValues(itemTypeId, enemyTypeId, rulesEvaluated, randomRollCount);
    return new FishingResultDecision(
      itemTypeId,
      hasItemDrop: true,
      enemyTypeId,
      hasEnemySpawn: enemyTypeId > 0,
      rulesEvaluated,
      randomRollCount,
      stoppedByRule);
  }

  public static FishingResultDecision NoResult(
    int enemyTypeId,
    int rulesEvaluated,
    int randomRollCount,
    bool stoppedByRule = false)
  {
    ValidateResultValues(0, enemyTypeId, rulesEvaluated, randomRollCount);
    return new FishingResultDecision(
      rolledItemDrop: 0,
      hasItemDrop: false,
      enemyTypeId,
      hasEnemySpawn: enemyTypeId > 0,
      rulesEvaluated,
      randomRollCount,
      stoppedByRule);
  }

  private static void ValidateResultValues(
    int itemTypeId,
    int enemyTypeId,
    int rulesEvaluated,
    int randomRollCount)
  {
    if (itemTypeId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(itemTypeId));
    }

    if (enemyTypeId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(enemyTypeId));
    }

    if (rulesEvaluated < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(rulesEvaluated));
    }

    if (randomRollCount < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(randomRollCount));
    }
  }
}
