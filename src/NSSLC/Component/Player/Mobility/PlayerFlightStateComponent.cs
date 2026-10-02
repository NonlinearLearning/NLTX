namespace Terraria.Player.Mobility;

// status: implemented-isolated-core
// crossSubsystemOwner: integration-review for Equipment, Jump, Carpet, Grapple, Mount, Spatial, and persistence
public sealed class PlayerFlightStateComponent
{
  public float WingTime { get; set; }

  public int WingTimeMax { get; set; }
}
