using Terraria.NonAuthoritative.Simulation;

namespace Terraria.NonAuthoritative.SimulationHost;

internal sealed class ActiveProjectileTickPhase : IWorldSimulationTickPhase
{
  private readonly IWorldSimulationTickPhase _phase;

  public ActiveProjectileTickPhase(RuntimeProjectileStore projectiles)
  {
    ArgumentNullException.ThrowIfNull(projectiles);
    _phase = projectiles.SimulationTickPhase;
  }

  public WorldSimulationPhase Phase => _phase.Phase;

  public void Execute(WorldSimulationTickContext context)
  {
    _phase.Execute(context);
  }
}
