namespace Terraria.Player;

public sealed class PlayerDebuffStatusAliasComponent
{
  public bool Cursed { get; internal set; }

  public bool Bleed { get; internal set; }

  public bool Confused { get; internal set; }

  public bool BrokenArmor { get; internal set; }

  public bool Silence { get; internal set; }

  public bool Slow { get; internal set; }

  public bool Gross { get; internal set; }

  public bool Tongued { get; internal set; }

  public void CommitGross(bool gross)
  {
    Gross = gross;
  }

  internal void ResetEffects()
  {
    Cursed = false;
    Bleed = false;
    Confused = false;
    BrokenArmor = false;
    Silence = false;
    Slow = false;
    Gross = false;
    Tongued = false;
  }
}
