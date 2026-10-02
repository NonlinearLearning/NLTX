using Terraria.WorldSession.Components;

namespace Terraria.WorldSession.Seeds;

public readonly record struct SpecialSeedRuleInput(
  SecretSeedFlags Flags,
  bool SurfaceIsDesert,
  bool NoSurface)
{
  public bool OnlyShimmerOceanWorlds =>
    Flags.DrunkWorld &&
    Flags.TenthAnniversaryWorld &&
    !Flags.RemixWorld &&
    !Flags.ZenithWorld &&
    !Flags.NotTheBeesWorld;
}
