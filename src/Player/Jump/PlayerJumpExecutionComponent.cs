namespace Terraria.Player.Jump;

// status: implemented-isolated-core
// crossSubsystemOwner: integration-review for Spatial, Mount, Presentation, and effects
public sealed class PlayerJumpExecutionComponent
{
  public bool IsPerformingDownDash { get; set; }

  public bool IsPerformingCloud { get; set; }

  public bool IsPerformingSandstorm { get; set; }

  public bool IsPerformingBlizzard { get; set; }

  public bool IsPerformingFart { get; set; }

  public bool IsPerformingSail { get; set; }

  public bool IsPerformingUnicorn { get; set; }

  public bool IsPerformingSantank { get; set; }

  public bool IsPerformingWallOfFleshGoat { get; set; }

  public bool IsPerformingBasilisk { get; set; }

  public bool IsPerformingPogostickTricks { get; set; }
}
