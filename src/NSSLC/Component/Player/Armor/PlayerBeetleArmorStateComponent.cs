namespace Terraria.Player.Armor;

// status: implemented-isolated-core
// crossSubsystemOwner: integration-review for Buff, Combat, presentation, network, and persistence
public sealed class PlayerBeetleArmorStateComponent
{
  public int BeetleOrbCount { get; set; }

  public float BeetleCounter { get; set; }

  public int BeetleCountdown { get; set; }

  public bool HasDefenseSet { get; set; }

  public bool HasOffenseSet { get; set; }

  public bool BeetleBuffActive { get; set; }
}
