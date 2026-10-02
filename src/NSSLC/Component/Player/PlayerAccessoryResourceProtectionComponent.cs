namespace Terraria.Player;

public sealed class PlayerAccessoryResourceProtectionComponent
{
  public static readonly float PhilosopherStoneDurationMultiplier = 0.75f;

  public bool LongInvince { get; internal set; }

  public bool PStone { get; internal set; }

  public bool ManaFlower { get; internal set; }

  public bool MoonLeech { get; internal set; }

  internal void ResetEffects()
  {
    LongInvince = false;
    PStone = false;
    ManaFlower = false;
    MoonLeech = false;
  }
}
