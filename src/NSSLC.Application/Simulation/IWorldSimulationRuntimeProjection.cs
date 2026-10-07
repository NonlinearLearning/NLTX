namespace Terraria.NonAuthoritative.Simulation;

/// <summary>Updates a compatibility view from the committed session snapshot.</summary>
public interface IWorldSimulationRuntimeProjection
{
  void Project(WorldSimulationTickContext context);
}
