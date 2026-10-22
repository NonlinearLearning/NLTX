namespace MinimalImplementation.Combat.Npcs;

/// <summary>Creates generation-bound packet 131 immunity updates from committed NPC state.</summary>
public sealed class NpcImmunityPacketProducer
{
  public NpcImmunityPacketIntent Create(
    NpcManager owner,
    NpcView npc,
    int immunityTime,
    short sourcePlayerIndex)
  {
    ArgumentNullException.ThrowIfNull(owner);
    ArgumentNullException.ThrowIfNull(npc);
    if (!owner.TryGet(npc.Slot, npc.Entity, out NpcView? current) ||
        current is null || current.Revision != npc.Revision ||
        current.CommitVersion != npc.CommitVersion)
    {
      throw new InvalidOperationException(
        "Packet 131 cannot target an NPC snapshot from an inactive or stale generation.");
    }

    if (!npc.Entity.IsValid || npc.Slot < 0 || npc.Revision <= 0 ||
        !npc.CommitVersion.IsValid)
    {
      throw new InvalidOperationException("Packet 131 requires a valid committed NPC snapshot.");
    }

    if (immunityTime < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(immunityTime));
    }

    if (sourcePlayerIndex < -1 || sourcePlayerIndex >= 255)
    {
      throw new ArgumentOutOfRangeException(nameof(sourcePlayerIndex));
    }

    return new NpcImmunityPacketIntent(
      npc.Entity,
      npc.Slot,
      npc.Revision,
      npc.CommitVersion,
      immunityTime,
      sourcePlayerIndex);
  }
}
