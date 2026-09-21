namespace Terraria.Player;

// status: implemented-isolated-core
// crossSubsystemOwner: integration-review for client input, UI and selection command routing
public struct PlayerInteractionUiStateComponent
{
  public bool CreativeInterface;
  public bool MouseInterface;
  public bool LastMouseInterface;
  public int NoThrow;
}
