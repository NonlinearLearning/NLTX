namespace Terraria.Player.Progression;

public static class PlayerMinionDamageHighWaterMarkSystem
{
  public static void AccumulateStormTigerGemOriginalDamage(
    PlayerMinionDamageHighWaterMarkComponent component,
    int originalDamage)
  {
    ArgumentNullException.ThrowIfNull(component);
    component.HighestStormTigerGemOriginalDamage = Math.Max(
      component.HighestStormTigerGemOriginalDamage,
      originalDamage);
  }

  public static void AccumulateAbigailCounterOriginalDamage(
    PlayerMinionDamageHighWaterMarkComponent component,
    int originalDamage)
  {
    ArgumentNullException.ThrowIfNull(component);
    component.HighestAbigailCounterOriginalDamage = Math.Max(
      component.HighestAbigailCounterOriginalDamage,
      originalDamage);
  }

  public static void Reset(PlayerMinionDamageHighWaterMarkComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    component.HighestStormTigerGemOriginalDamage = 0;
    component.HighestAbigailCounterOriginalDamage = 0;
  }
}
