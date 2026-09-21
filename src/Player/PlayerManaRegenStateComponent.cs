namespace Terraria.Player;

public sealed class PlayerManaRegenStateComponent
{
  public int ManaRegen { get; internal set; }

  public int ManaRegenCount { get; internal set; }

  public float ManaRegenDelay { get; internal set; }

  public bool ManaRegenBuff { get; internal set; }

  internal void ResetEffects()
  {
    ManaRegen = 0;
    ManaRegenBuff = false;
  }

  internal void ResetForLifecycle()
  {
    ManaRegen = 0;
    ManaRegenCount = 0;
    ManaRegenDelay = 0f;
    ManaRegenBuff = false;
  }
}
