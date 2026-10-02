namespace Terraria.Player;

public sealed class PlayerInteractionPolicyComponent
{
  public bool PreventAllItemPickups { get; internal set; }

  public bool DontHurtCritters { get; internal set; }

  public bool HasLucyTheAxe { get; internal set; }

  public bool DontHurtNature { get; internal set; }

  internal void ResetEffects()
  {
    PreventAllItemPickups = false;
    DontHurtCritters = false;
    HasLucyTheAxe = false;
    DontHurtNature = false;
  }
}
