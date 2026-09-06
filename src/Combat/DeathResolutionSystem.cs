namespace Terraria.Combat;

public static class DeathResolutionSystem
{
  public static DeathResolution Resolve(
    in HealthComponent health,
    in DamageAttribution attribution)
  {
    return new DeathResolution(
      health.Current <= 0,
      new DeathCause(attribution.Kind, attribution.Source));
  }
}
