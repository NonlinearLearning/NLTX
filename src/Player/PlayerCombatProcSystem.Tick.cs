namespace Terraria.Player;

public sealed partial class PlayerCombatProcSystem
{
  private static readonly PlayerCombatProcTickQuery TickQuery = new();

  public void AdvanceTick(bool expertMode)
  {
    PlayerCombatProcTickResult result = TickQuery.Advance(
      new PlayerCombatProcTickInput(
        _component.GhostDmg,
        _component.LifeSteal,
        _component.EocDash,
        _component.EocHit,
        _component.InfernoCounter,
        _component.StarCloakCooldown,
        _component.TitaniumStormCooldown,
        _component.PetalTimer,
        _component.BoneGloveTimer,
        expertMode));

    _component.GhostDmg = result.GhostDmg;
    _component.LifeSteal = result.LifeSteal;
    _component.EocDash = result.EocDash;
    _component.EocHit = result.EocHit;
    _component.InfernoCounter = result.InfernoCounter;
    _component.StarCloakCooldown = result.StarCloakCooldown;
    _component.TitaniumStormCooldown = result.TitaniumStormCooldown;
    _component.PetalTimer = result.PetalTimer;
    _component.BoneGloveTimer = result.BoneGloveTimer;
  }

  public void BeginEocDash(int durationTicks)
  {
    _component.EocDash = Math.Max(0, durationTicks);
    if (_component.EocDash == 0)
    {
      _component.EocHit = -1;
    }
  }

  public bool CommitEocDashHit(int targetId)
  {
    if (_component.EocDash <= 0 || targetId < 0)
    {
      return false;
    }

    _component.EocHit = targetId;
    return true;
  }

  public void RecordPhantomPhoneixLaunch(int count = 1)
  {
    if (count <= 0)
    {
      return;
    }

    _component.PhantomPhoneixCounter = SaturatingAdd(
      _component.PhantomPhoneixCounter,
      count);
  }

  private static int SaturatingAdd(int current, int additional)
  {
    return current > int.MaxValue - additional
      ? int.MaxValue
      : current + additional;
  }
}
