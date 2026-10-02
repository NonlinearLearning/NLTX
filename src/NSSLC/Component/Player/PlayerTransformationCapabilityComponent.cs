namespace Terraria.Player;

public sealed class PlayerTransformationCapabilityComponent
{
  public bool WereWolf { get; internal set; }

  public bool WolfAcc { get; internal set; }

  public bool HideMerman { get; internal set; }

  public bool HideWolf { get; internal set; }

  public bool ForceMerman { get; internal set; }

  public bool ForceWerewolf { get; internal set; }

  public bool AccMerman { get; internal set; }

  public bool Merman { get; internal set; }

  internal void ResetEffects()
  {
    WereWolf = false;
    WolfAcc = false;
    HideMerman = false;
    HideWolf = false;
    ForceMerman = false;
    ForceWerewolf = false;
    AccMerman = false;
    Merman = false;
  }
}
