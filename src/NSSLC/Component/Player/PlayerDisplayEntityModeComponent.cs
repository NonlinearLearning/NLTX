namespace Terraria.Player;

// status: implemented-isolated-core
// source-members: P09-741, P09-742
// crossSubsystemOwner: display entity setup and rendering remain integration-review
public sealed class PlayerDisplayEntityModeComponent
{
  public bool IsDisplayDollOrInanimate { get; internal set; }

  public bool IsHatRackDoll { get; internal set; }
}
