namespace Terraria.Player;

public sealed class PlayerAccessoryMeleeCapabilityComponent
{
  public bool KbGlove { get; internal set; }

  public bool AutoReuseGlove { get; internal set; }

  public bool MeleeScaleGlove { get; internal set; }

  public bool KbBuff { get; internal set; }

  internal void ResetEffects()
  {
    KbGlove = false;
    AutoReuseGlove = false;
    MeleeScaleGlove = false;
    KbBuff = false;
  }
}
