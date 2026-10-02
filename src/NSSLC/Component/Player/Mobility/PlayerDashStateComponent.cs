namespace Terraria.Player.Mobility;

// status: implemented-isolated-core
// crossSubsystemOwner: integration-review for input, equipment, Spatial, effects, network, and persistence
public sealed class PlayerDashStateComponent
{
  public int DashType { get; set; }

  public int ActiveDash { get; set; }

  public int DashTime { get; set; }

  public int TimeSinceLastDashStarted { get; set; }

  public int DashDelay { get; set; }
}
