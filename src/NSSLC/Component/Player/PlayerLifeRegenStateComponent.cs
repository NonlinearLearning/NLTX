namespace Terraria.Player;

public sealed class PlayerLifeRegenStateComponent
{
  public int LifeRegen { get; internal set; }

  public int LifeRegenCount { get; internal set; }

  public float LifeRegenTime { get; internal set; }

  internal void ResetEffects()
  {
    LifeRegen = 0;
  }

  internal void ResetForLifecycle()
  {
    LifeRegen = 0;
    LifeRegenCount = 0;
    LifeRegenTime = 0f;
  }
}
