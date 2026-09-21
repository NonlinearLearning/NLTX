namespace Terraria.Dome.Simulation.Tick;

public enum SimulationTickPhase
{
  BeginTick,
  ApplyWorldClock,
  ApplyPlayerInputs,
  ApplyPlayerControl,
  ResolveTileCollision,
  SelectNpcTargets,
  ApplyNpcAi,
  MoveEntities,
  AdvanceProjectiles,
  ResolveCombat,
  CommitDomainCommands,
  PublishSnapshot,
  EndTick
}
