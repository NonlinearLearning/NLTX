namespace Terraria.Player;

public sealed class PlayerAmmoCostPolicyComponent
{
  public bool ChloroAmmoCost80 { get; internal set; }

  public bool HuntressAmmoCost90 { get; internal set; }

  public bool AmmoCost80 { get; internal set; }

  public bool AmmoCost75 { get; internal set; }

  public bool AmmoBox { get; internal set; }

  public bool AmmoPotion { get; internal set; }

  internal void ResetEffects()
  {
    ChloroAmmoCost80 = false;
    HuntressAmmoCost90 = false;
    AmmoCost80 = false;
    AmmoCost75 = false;
    AmmoBox = false;
    AmmoPotion = false;
  }
}
