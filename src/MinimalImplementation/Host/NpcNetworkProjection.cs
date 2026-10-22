using MinimalImplementation.Combat.Identity;
using MinimalImplementation.Combat.Npcs;
using MinimalImplementation.Contracts;

namespace MinimalImplementation.Server.Host;

/// <summary>Owner-loop projection from NPC identities to the current client's byte generations.</summary>
public sealed class NpcNetworkProjection {
  private readonly Dictionary<short, (CombatEntityIdentity Entity, byte Generation)> _slots = [];
  private readonly NpcPacketPayloadCodec _codec = new(
      new NpcPacket23ProtocolFacts(new bool[1024], (_, _, _) => 4));
  private readonly NpcSyncPacketProducer _producer = new();

  public byte GetGeneration(NpcView npc) {
    if (_slots.TryGetValue(npc.Slot, out var current)) {
      if (current.Entity == npc.Entity) {
        return current.Generation;
      }
      if (current.Generation == byte.MaxValue) {
        throw new InvalidOperationException("NPC wire generation exhausted; restart the session.");
      }
      byte next = checked((byte)(current.Generation + 1));
      _slots[npc.Slot] = (npc.Entity, next);
      return next;
    }
    _slots.Add(npc.Slot, (npc.Entity, 1));
    return 1;
  }

  public bool Matches(NpcView npc, byte generation) {
    return _slots.TryGetValue(npc.Slot, out var current) &&
        current.Entity == npc.Entity && current.Generation == generation;
  }

  public OutboundPacketIntent CreateSync(
      NpcView npc, IReadOnlyList<SessionEpoch> targets, bool initial = false) {
    // These supported actors are never catchable. Unknown content needs its own protocol facts.
    if (npc.Type is not (1 or 2 or 3 or 22)) {
      throw new NotSupportedException("NPC synchronization facts are missing for this type.");
    }
    byte[] body = _codec.EncodeSyncBody(npc, _producer, GetGeneration(npc), initial);
    return new OutboundPacketIntent(targets, null, 23, body, npc.CommitVersion);
  }
}
