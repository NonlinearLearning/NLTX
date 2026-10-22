using System.Numerics;
using MinimalImplementation.Combat.Identity;
using MinimalImplementation.Contracts;

namespace MinimalImplementation.Combat.Npcs;

public sealed partial class NpcManager
{
  /// <summary>
  /// Prepares the server side of packet 92. The packet carries an increment; the committed
  /// value is the accumulated result, matching MessageBuffer's server branch.
  /// </summary>
  public NpcPreparation<NpcExtraValuePlan> PrepareExtraValue(
      CombatEntityIdentity identity,
      long expectedRevision,
      int increment,
      Vector2 pingPosition)
  {
    if (!IsWorldScopeCurrent() ||
        !_byIdentity.TryGetValue(identity, out Npc? npc) ||
        !npc.IsActive ||
        npc.Revision != expectedRevision ||
        !_identityAuthority.IsActive(identity))
    {
      return NpcPreparation<NpcExtraValuePlan>.Rejected(
          ProtocolErrorCode.StaleEntity,
          "Packet 92 targets a stale NPC generation.");
    }

    if (!float.IsFinite(pingPosition.X) || !float.IsFinite(pingPosition.Y))
    {
      return NpcPreparation<NpcExtraValuePlan>.Rejected(
          ProtocolErrorCode.InvalidLength,
          "Packet 92 money-ping coordinates must be finite.");
    }

    long next = (long)npc.ExtraValue + increment;
    if (next < int.MinValue || next > int.MaxValue)
    {
      return NpcPreparation<NpcExtraValuePlan>.Rejected(
          ProtocolErrorCode.InvalidLength,
          "Packet 92 extra value would overflow the Int32 accumulator.");
    }

    return NpcPreparation<NpcExtraValuePlan>.Prepared(
        new NpcExtraValuePlan(_revision, npc, expectedRevision, (int)next, pingPosition));
  }

  public OwnerCommitResult Validate(NpcExtraValuePlan plan)
  {
    ArgumentNullException.ThrowIfNull(plan);
    if (!IsWorldScopeCurrent() || plan.ManagerRevision != _revision ||
        !plan.Npc.IsActive || plan.Npc.Revision != plan.ExpectedNpcRevision ||
        plan.Npc.Slot < 0 || plan.Npc.Slot >= _slots.Length ||
        _slots[plan.Npc.Slot] != plan.Npc ||
        !_byIdentity.TryGetValue(plan.Npc.Entity, out Npc? current) ||
        current != plan.Npc || !_identityAuthority.IsActive(plan.Npc.Entity))
    {
      return OwnerCommitResult.Rejected(
          ProtocolErrorCode.StaleEntity,
          "NPC extra-value preparation changed or expired.");
    }

    if (_revision == long.MaxValue || plan.Npc.Revision == long.MaxValue)
    {
      return OwnerCommitResult.Rejected(
          ProtocolErrorCode.NotReady,
          "NPC extra-value revision range is exhausted.");
    }

    return OwnerCommitResult.Applied(_commitVersion);
  }

  public (OwnerCommitResult Result, NpcView? Npc) Commit(NpcExtraValuePlan plan)
  {
    ArgumentNullException.ThrowIfNull(plan);
    OwnerCommitResult validation = Validate(plan);
    if (validation.Status != OwnerCommitStatus.Applied)
    {
      return (validation, null);
    }

    CommitVersion nextVersion;
    try
    {
      nextVersion = _commitVersion.Next();
    }
    catch (InvalidOperationException)
    {
      return (OwnerCommitResult.Rejected(
          ProtocolErrorCode.NotReady,
          "NPC extra-value commit version range is exhausted."), null);
    }

    plan.Npc.ApplyExtraValue(plan.NextExtraValue, plan.Position, nextVersion);
    CommitMutation(nextVersion);
    return (OwnerCommitResult.Applied(nextVersion), plan.Npc.Capture());
  }
}
