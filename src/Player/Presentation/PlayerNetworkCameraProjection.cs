using Terraria.Player;

namespace Terraria.Player.Presentation;

public sealed class PlayerNetworkCameraProjection
{
  public PlayerCameraSnapshot Project(
    PlayerNetworkCameraStateComponent component,
    SimulationTick tick)
  {
    ArgumentNullException.ThrowIfNull(component);
    return component.ToSnapshot(tick);
  }
}
