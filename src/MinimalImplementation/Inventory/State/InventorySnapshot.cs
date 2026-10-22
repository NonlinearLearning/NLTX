using MinimalImplementation.Contracts;

namespace MinimalImplementation.Server.Inventory.State;

public sealed record InventorySnapshot
{
  public EntityUuid PlayerId { get; }
  public CommitVersion Revision { get; }
  public IReadOnlyList<InventorySlotSnapshot> Slots { get; }

  public InventorySnapshot(
    EntityUuid playerId,
    CommitVersion revision,
    IReadOnlyCollection<InventorySlotSnapshot> slots)
  {
    if (!playerId.IsValid)
    {
      throw new ArgumentException("An inventory snapshot requires a valid player identity.", nameof(playerId));
    }

    if (!revision.IsValid)
    {
      throw new ArgumentException("An inventory snapshot requires a committed revision.", nameof(revision));
    }

    ArgumentNullException.ThrowIfNull(slots);
    PlayerId = playerId;
    Revision = revision;
    Slots = Array.AsReadOnly(slots.ToArray());
  }
}
