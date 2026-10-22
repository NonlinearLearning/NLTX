namespace MinimalImplementation.Combat.Npcs;

/// <summary>Prepared damage calculation bound to an exact NPC and owner revision.</summary>
public sealed class NpcDamagePlan {
  internal NpcDamagePlan(NpcStatePlan state, NpcDamageRequest request, int damage) {
    State = state;
    Request = request;
    Damage = damage;
  }

  internal NpcStatePlan State { get; }
  internal NpcDamageRequest Request { get; }
  public int Damage { get; }
  public bool IsLethal => State.Update.Life == 0;

  /// <summary>
  /// Project after successful commit. Null means the input exceeds packet 28's positive
  /// Int16 range; publish packet 23 instead of wrapping into a negative damage command.
  /// </summary>
  public NpcDamagePacketIntent? CreateDamagePacket(NpcView committed) {
    ArgumentNullException.ThrowIfNull(committed);
    if (committed.Entity != Request.Target || committed.Slot != State.Npc.Slot ||
        committed.Revision != State.ExpectedNpcRevision + 1 ||
        committed.Life != State.Update.Life || committed.Velocity != State.Update.Velocity ||
        !committed.IsDirty || !committed.CommitVersion.IsValid) {
      throw new InvalidOperationException("Packet 28 requires this damage plan's committed result.");
    }
    return Request.Damage <= short.MaxValue ? new NpcDamagePacketIntent(Request, committed) : null;
  }
}
