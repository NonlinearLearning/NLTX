using Terraria.NonAuthoritative.Persistence;
using Terraria.NonAuthoritative.Simulation;
using Terraria.NonAuthoritative.WorldStorage;

namespace Terraria.NonAuthoritative.SimulationHost;

internal sealed class ActiveRuntimeProjection : IWorldSimulationRuntimeProjection
{
  public void Project(WorldSimulationTickContext context)
  {
    ArgumentNullException.ThrowIfNull(context);
    WorldStorageOperationResult result =
      LegacyWorldSessionProjection.PublishRuntimeTickState(context.Session);
    if (!result.Succeeded)
    {
      throw new InvalidOperationException(
        $"The active world state could not be projected to the legacy runtime: " +
        $"{result.Failure.Kind} {result.Failure.Detail}");
    }
  }
}
