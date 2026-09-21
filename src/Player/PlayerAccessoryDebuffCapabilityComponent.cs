namespace Terraria.Player;

public sealed class PlayerAccessoryDebuffCapabilityComponent
{
  public bool VortexDebuff { get; internal set; }

  public bool TrapDebuffSource { get; internal set; }

  public bool WitheredArmor { get; internal set; }

  public bool WitheredWeapon { get; internal set; }

  public bool SlowOgreSpit { get; internal set; }

  public bool ParryDamageBuff { get; internal set; }

  public bool BallistaPanic { get; internal set; }

  internal void ResetEffects()
  {
    VortexDebuff = false;
    TrapDebuffSource = false;
    WitheredArmor = false;
    WitheredWeapon = false;
    SlowOgreSpit = false;
    ParryDamageBuff = false;
    BallistaPanic = false;
  }
}
