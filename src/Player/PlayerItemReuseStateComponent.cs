namespace Terraria.Player;

// status: implemented-isolated-core
// crossSubsystemOwner: integration-review for ItemCheck and reuse scheduling
public struct PlayerItemReuseStateComponent
{
  public int ReuseDelayRemainingTicks;
  public bool PendingItemReuse;
}
