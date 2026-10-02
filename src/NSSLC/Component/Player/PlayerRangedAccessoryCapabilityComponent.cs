namespace Terraria.Player;

public sealed class PlayerRangedAccessoryCapabilityComponent
{
  public bool MagicQuiver { get; internal set; }

  public bool MagmaStone { get; internal set; }

  public bool LavaRose { get; internal set; }

  public bool HasMoltenQuiver { get; internal set; }

  public int PhantasmTime { get; internal set; }

  internal void ResetEffects()
  {
    MagicQuiver = false;
    MagmaStone = false;
    LavaRose = false;
    HasMoltenQuiver = false;
  }

  internal void ResetForLifecycle()
  {
    MagicQuiver = false;
    MagmaStone = false;
    LavaRose = false;
    HasMoltenQuiver = false;
    PhantasmTime = 0;
  }
}
