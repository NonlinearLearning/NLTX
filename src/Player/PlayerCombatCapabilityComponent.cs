namespace Terraria.Player;

public sealed class PlayerCombatCapabilityComponent
{
  public bool SpaceGun { get; internal set; }

  public bool ChaosState { get; internal set; }

  public bool StrongBees { get; internal set; }

  public bool SporeSac { get; internal set; }

  public bool ShinyStone { get; internal set; }

  public bool EmpressBrooch { get; internal set; }

  public bool VolatileGelatin { get; internal set; }

  public int VolatileGelatinCounter { get; internal set; }

  public bool HasMagiluminescence { get; internal set; }

  public bool ShadowArmor { get; internal set; }

  internal void ResetEffects()
  {
    SpaceGun = false;
    ChaosState = false;
    StrongBees = false;
    SporeSac = false;
    ShinyStone = false;
    EmpressBrooch = false;
    VolatileGelatin = false;
    VolatileGelatinCounter = 0;
    HasMagiluminescence = false;
    ShadowArmor = false;
  }
}
