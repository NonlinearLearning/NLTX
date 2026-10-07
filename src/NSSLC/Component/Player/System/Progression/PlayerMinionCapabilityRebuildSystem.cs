namespace Terraria.Player.Progression;

public static class PlayerMinionCapabilityRebuildSystem
{
  // The caller resolves current-tick flags and owns their ordering with buff/content updates.
  public static void Rebuild(
    in PlayerMinionCapabilityRebuildInput input,
    PlayerCoreMinionCapabilityComponent core,
    PlayerCrossoverMinionCapabilityComponent crossover)
  {
    ArgumentNullException.ThrowIfNull(core);
    ArgumentNullException.ThrowIfNull(crossover);

    core.Pygmy = input.Pygmy;
    core.Raven = input.Raven;
    core.Slime = input.Slime;
    core.HornetMinion = input.HornetMinion;
    core.ImpMinion = input.ImpMinion;
    core.TwinsMinion = input.TwinsMinion;
    core.SpiderMinion = input.SpiderMinion;
    core.PirateMinion = input.PirateMinion;
    core.SharknadoMinion = input.SharknadoMinion;
    core.UfoMinion = input.UfoMinion;
    core.DeadlySphereMinion = input.DeadlySphereMinion;
    core.StardustMinion = input.StardustMinion;
    core.StardustGuardian = input.StardustGuardian;
    core.StardustDragon = input.StardustDragon;
    core.BatsOfLight = input.BatsOfLight;
    core.BabyBird = input.BabyBird;
    core.VampireFrog = input.VampireFrog;
    core.StormTiger = input.StormTiger;
    core.Smolstar = input.Smolstar;
    core.EmpressBlade = input.EmpressBlade;
    core.FlinxMinion = input.FlinxMinion;
    core.AbigailMinion = input.AbigailMinion;

    crossover.DeadCellsMushroomBoiMinion = input.DeadCellsMushroomBoiMinion;
    crossover.PalworldCattivaMinion = input.PalworldCattivaMinion;
    crossover.PalworldFoxsparksMinion = input.PalworldFoxsparksMinion;
  }

  public static void Reset(
    PlayerCoreMinionCapabilityComponent core,
    PlayerCrossoverMinionCapabilityComponent crossover)
  {
    Rebuild(default, core, crossover);
  }
}
