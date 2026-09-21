namespace Terraria.Dome.Simulation.Player.Components;

public struct PlayerDefenseStateComponent
{
  public const sbyte NoShield = -1;

  public bool DefendedByPaladin;
  public bool HasPaladinShield;
  public sbyte Shield;
  public bool ShieldRaised;
  public int ShieldParryTimeLeft;

  public PlayerDefenseStateComponent()
  {
    DefendedByPaladin = false;
    HasPaladinShield = false;
    Shield = NoShield;
    ShieldRaised = false;
    ShieldParryTimeLeft = 0;
  }

  public void ClearTransientState()
  {
    DefendedByPaladin = false;
    ShieldRaised = false;
    ShieldParryTimeLeft = 0;
  }
}
