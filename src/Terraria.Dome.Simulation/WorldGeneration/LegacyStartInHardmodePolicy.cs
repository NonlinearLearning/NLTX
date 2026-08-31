namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct LegacyStartInHardmodeResult(
  WorldRuleSnapshotComponent Rules,
  bool InitializeHardmode);

public static class LegacyStartInHardmodePolicy
{
  public static LegacyStartInHardmodeResult Apply(WorldRuleSnapshotComponent rules)
  {
    return new LegacyStartInHardmodeResult(
      new WorldRuleSnapshotComponent(
        rules.Difficulty,
        rules.SecretSeedVariant,
        isHardmode: true),
      InitializeHardmode: true);
  }
}
