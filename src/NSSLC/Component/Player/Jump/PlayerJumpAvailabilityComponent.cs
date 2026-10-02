namespace Terraria.Player.Jump;

// status: implemented-isolated-core
// crossSubsystemOwner: integration-review for Equipment, Buff, Mount, Mobility, and persistence
public sealed class PlayerJumpAvailabilityComponent
{
  public bool HasCloudOption { get; set; }

  public bool CanJumpAgainCloud { get; set; }

  public bool HasSandstormOption { get; set; }

  public bool CanJumpAgainSandstorm { get; set; }

  public bool HasBlizzardOption { get; set; }

  public bool CanJumpAgainBlizzard { get; set; }

  public bool HasFartOption { get; set; }

  public bool CanJumpAgainFart { get; set; }

  public bool HasSailOption { get; set; }

  public bool CanJumpAgainSail { get; set; }

  public bool HasUnicornOption { get; set; }

  public bool CanJumpAgainUnicorn { get; set; }

  public bool HasSantankOption { get; set; }

  public bool CanJumpAgainSantank { get; set; }

  public bool HasWallOfFleshGoatOption { get; set; }

  public bool CanJumpAgainWallOfFleshGoat { get; set; }

  public bool HasBasiliskOption { get; set; }

  public bool CanJumpAgainBasilisk { get; set; }
}
