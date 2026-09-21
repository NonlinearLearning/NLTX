namespace Terraria.Player;

// status: implemented-isolated-core
// source-member: P10-828 isOperatingAnotherEntity
// crossSubsystemOwner: integration-review for entity interaction claim/release ownership
public struct PlayerEntityInteractionLockStateComponent
{
  public bool IsOperatingAnotherEntity;
}
