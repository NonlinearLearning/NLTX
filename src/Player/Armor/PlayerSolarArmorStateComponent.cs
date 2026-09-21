namespace Terraria.Player.Armor;

// status: implemented-isolated-core
// crossSubsystemOwner: integration-review for Buff, Combat, Spatial, network, and persistence
public sealed class PlayerSolarArmorStateComponent
{
  public int ShieldCount { get; set; }

  public int SolarCounter { get; set; }

  public bool IsSolarDashing { get; set; }

  public bool SolarDashConsumedFlare { get; set; }
}
