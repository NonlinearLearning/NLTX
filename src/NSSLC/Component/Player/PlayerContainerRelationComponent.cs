namespace Terraria.Player;

// status: implemented-isolated-core
// source-members: P09-746, P09-747, P09-748, P09-749, P09-750
// crossSubsystemOwner: container contents, capacity, and transfer ordering remain Items-owned
public sealed class PlayerContainerRelationComponent
{
  public PlayerContainerRef Bank { get; internal set; } = PlayerContainerRef.None;

  public PlayerContainerRef Bank2 { get; internal set; } = PlayerContainerRef.None;

  public PlayerContainerRef Bank3 { get; internal set; } = PlayerContainerRef.None;

  public PlayerContainerRef Bank4 { get; internal set; } = PlayerContainerRef.None;

  public VoidVaultState VoidVaultState { get; internal set; }
}
