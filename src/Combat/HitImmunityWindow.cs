namespace Terraria.Combat;

public struct HitImmunityWindow
{
  public HitImmunityWindow(
    HitImmunityKey key,
    int remainingTicks)
  {
    Key = key;
    RemainingTicks = remainingTicks;
  }

  public HitImmunityKey Key;
  public int RemainingTicks;

  public bool IsActive => RemainingTicks > 0;
}
