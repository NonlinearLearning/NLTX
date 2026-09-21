namespace Terraria.Dome.Simulation.Player.Systems;

public readonly record struct PlayerSleepWakeInput(
  bool IsSleeping,
  int ItemAnimationTicks,
  int ItemDamage,
  bool NoMelee);

public static class PlayerSleepWakePolicy
{
  public static bool ShouldWakeForItemUse(PlayerSleepWakeInput input)
  {
    return input.IsSleeping && input.ItemAnimationTicks > 0 && input.ItemDamage > 0 &&
      !input.NoMelee;
  }
}
