namespace Terraria.NonAuthoritative.Simulation;

public interface IWorldSimulationTickPhase
{
  WorldSimulationPhase Phase { get; }

  void Execute(WorldSimulationTickContext context);
}
