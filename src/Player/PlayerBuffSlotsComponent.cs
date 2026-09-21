namespace Terraria.Player;

// status: implemented-isolated-core
// source-members: P09-716, P09-717
// crossSubsystemOwner: buff lifecycle, replication, and persistence remain integration-review
public sealed class PlayerBuffSlotsComponent
{
  public const int MaximumSlotCount = 44;

  public BuffSlot[] Slots { get; } = new BuffSlot[MaximumSlotCount];
}
