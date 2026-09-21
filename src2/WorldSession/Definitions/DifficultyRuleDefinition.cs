namespace Terraria.WorldSession.Definitions;

public static class DifficultyRuleDefinition
{
  public static DifficultyCurve EnemyMaxLifeMultiplier { get; } = new(new[]
  {
    new DifficultyCurveKey(DifficultyLevelDefinition.Journey, 0.5f),
    new DifficultyCurveKey(DifficultyLevelDefinition.Legendary, 4f)
  });

  public static DifficultyCurve EnemyDamageMultiplier { get; } = new(new[]
  {
    new DifficultyCurveKey(DifficultyLevelDefinition.Journey, 0.5f),
    new DifficultyCurveKey(DifficultyLevelDefinition.Master, 3f),
    new DifficultyCurveKey(DifficultyLevelDefinition.Legendary, 5.3333335f)
  });

  public static DifficultyCurve HostileProjectileDamageMultiplier { get; } = new(new[]
  {
    new DifficultyCurveKey(DifficultyLevelDefinition.Journey, 0.5f),
    new DifficultyCurveKey(DifficultyLevelDefinition.Master, 3f)
  });

  public static DifficultyCurve KnockbackToEnemiesMultiplier { get; } = new(new[]
  {
    new DifficultyCurveKey(DifficultyLevelDefinition.Classic, 1f),
    new DifficultyCurveKey(DifficultyLevelDefinition.Master, 0.8f)
  });

  public static DifficultyCurve EnemyMoneyDropMultiplier { get; } = new(new[]
  {
    new DifficultyCurveKey(DifficultyLevelDefinition.Classic, 1f),
    new DifficultyCurveKey(DifficultyLevelDefinition.Expert, 2.5f),
    new DifficultyCurveKey(DifficultyLevelDefinition.Master, 2.5f),
    new DifficultyCurveKey(DifficultyLevelDefinition.Legendary, 3.5f)
  });

  public static DifficultyCurve TownNpcDamageMultiplier { get; } = new(new[]
  {
    new DifficultyCurveKey(DifficultyLevelDefinition.Journey, 2f),
    new DifficultyCurveKey(DifficultyLevelDefinition.Classic, 1f),
    new DifficultyCurveKey(DifficultyLevelDefinition.Expert, 1.5f),
    new DifficultyCurveKey(DifficultyLevelDefinition.Legendary, 2f)
  });

  public static DifficultyCurve DebuffTimeMultiplier { get; } = new(new[]
  {
    new DifficultyCurveKey(DifficultyLevelDefinition.Classic, 1f),
    new DifficultyCurveKey(DifficultyLevelDefinition.Expert, 2f),
    new DifficultyCurveKey(DifficultyLevelDefinition.Master, 2.5f)
  });

  public static DifficultyCurve LightningPlayerDamageScaling { get; } = new(new[]
  {
    new DifficultyCurveKey(DifficultyLevelDefinition.Journey, 0.04f),
    new DifficultyCurveKey(DifficultyLevelDefinition.Classic, 0.08f),
    new DifficultyCurveKey(DifficultyLevelDefinition.Master, 0.24f),
    new DifficultyCurveKey(DifficultyLevelDefinition.Legendary, 0.4f)
  });
}
