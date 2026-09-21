namespace Terraria.Player.Accessories;

// status: implemented-isolated-core
// crossSubsystemOwner: integration-review for Item, WorldInteraction, Jump, NPC, Combat, and Lighting
public sealed class PlayerUtilityCapabilityComponent
{
  public bool ManaMagnet { get; set; }

  public bool LifeMagnet { get; set; }

  public bool TreasureMagnet { get; set; }

  public bool ChiselSpeed { get; set; }

  public bool LifeForce { get; set; }

  public bool HasDeadCellsDownDash { get; set; }

  public bool Calmed { get; set; }

  public bool Inferno { get; set; }
}
