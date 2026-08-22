using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldModel.Systems;

public sealed class WorldTimeRateSystem
{
  private readonly WorldTimeRatePolicy _policy = new();

  public WorldTimeRateSnapshot Resolve(WorldTimeRateInput input)
  {
    return _policy.Resolve(input);
  }
}
