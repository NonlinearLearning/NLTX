namespace Terraria.Player.Armor;

// status: implemented-isolated-core
// crossSubsystemOwner: integration-review for Resource, Combat, Buff, network, and persistence
public sealed class PlayerNebulaResourceStateComponent
{
  public int LifeLevel { get; set; }

  public int ManaLevel { get; set; }

  public int NebulaManaCounter { get; set; }

  public int DamageLevel { get; set; }
}
