namespace Terraria.Player.Jump;

// status: implemented-isolated-core
// crossSubsystemOwner: integration-review for Equipment, Spatial, Collision, and persistence
public sealed class PlayerJumpMobilityModifiersComponent
{
  public int DownDashTime { get; set; }

  public bool AutoJump { get; set; }

  public bool JustJumped { get; set; }

  public float JumpSpeedBoost { get; set; }

  public int ExtraFall { get; set; }
}
