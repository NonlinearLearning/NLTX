namespace Terraria.Player;

public sealed class PlayerElementalStatusComponent
{
  public bool Archery { get; internal set; }

  public bool Poisoned { get; internal set; }

  public bool Venom { get; internal set; }

  public bool Blind { get; internal set; }

  public bool Blackout { get; internal set; }

  public bool Headcovered { get; internal set; }

  public bool FrostBurn { get; internal set; }

  public bool OnFrostBurn { get; internal set; }

  public bool OnFrostBurn2 { get; internal set; }

  public bool Burned { get; internal set; }

  public bool Suffocating { get; internal set; }

  public bool OnFire { get; internal set; }

  public bool OnFire2 { get; internal set; }

  public bool OnFire3 { get; internal set; }

  internal void ResetEffects()
  {
    Archery = false;
    Poisoned = false;
    Venom = false;
    Blind = false;
    Blackout = false;
    Headcovered = false;
    FrostBurn = false;
    OnFrostBurn = false;
    OnFrostBurn2 = false;
    Burned = false;
    Suffocating = false;
    OnFire = false;
    OnFire2 = false;
    OnFire3 = false;
  }
}
