using System.Numerics;
using MinimalImplementation.Combat.Identity;
using MinimalImplementation.Contracts;

namespace MinimalImplementation.Combat.Npcs;

public sealed partial class NpcManager {
  public bool TryGet(CombatEntityIdentity identity, out NpcView? view) {
    view = null;
    if (!IsWorldScopeCurrent() || !_byIdentity.TryGetValue(identity, out Npc? npc) ||
        !npc.IsActive || !_identityAuthority.IsActive(identity)) {
      return false;
    }
    view = npc.Capture();
    return true;
  }

  public NpcPreparation<NpcDamagePlan> PrepareDamage(NpcDamageRequest request) {
    ArgumentNullException.ThrowIfNull(request);
    if (!IsWorldScopeCurrent() || !_byIdentity.TryGetValue(request.Target, out Npc? npc) ||
        !npc.IsActive || !_identityAuthority.IsActive(request.Target) ||
        npc.Revision != request.ExpectedRevision || npc.Life <= 0) {
      return NpcPreparation<NpcDamagePlan>.Rejected(
          ProtocolErrorCode.StaleEntity, "NPC hit requires a current living generation and revision.");
    }
    if (npc.Type is not (1 or 2 or 3)) {
      return NpcPreparation<NpcDamagePlan>.Rejected(
          ProtocolErrorCode.NotReady, "This NPC's type-specific StrikeNPC rules are not migrated.");
    }
    if (npc.DontTakeDamage || npc.Friendly) {
      return NpcPreparation<NpcDamagePlan>.Rejected(
          ProtocolErrorCode.Forbidden, "The NPC is not an eligible hostile damage target.");
    }
    if (request.Damage <= 0 || request.HitDirection is < -1 or > 1 ||
        !float.IsFinite(request.Knockback) || request.Knockback < 0f ||
        request.InteractionEpoch is { IsValid: false }) {
      return NpcPreparation<NpcDamagePlan>.Rejected(
          ProtocolErrorCode.InvalidLength, "NPC hit facts are invalid.");
    }
    int damage = NpcDamageRules.CalculateDamage(request.Damage, npc.Defense, request.Critical);
    Vector2 velocity = NpcDamageRules.Knockback(
        npc.Velocity, npc.KnockbackResist, npc.MaximumLife, damage, request);
    NpcStateUpdate update = npc.CreateStateUpdate(
        npc.Position, velocity, npc.Target, npc.DirectionPositive, npc.DirectionYPositive,
        npc.SpriteDirectionPositive, npc.Ai, Math.Max(0, npc.Life - damage));
    NpcPreparation<NpcStatePlan> state = PrepareStateUpdate(request.Target, update);
    if (!state.IsPrepared || state.Plan is null) {
      return NpcPreparation<NpcDamagePlan>.Rejected(
          state.Result.ErrorCode ?? ProtocolErrorCode.Conflict,
          state.Result.Detail ?? "NPC damage state could not be prepared.");
    }
    return NpcPreparation<NpcDamagePlan>.Prepared(new NpcDamagePlan(state.Plan, request, damage));
  }

  public OwnerCommitResult Validate(NpcDamagePlan plan) {
    ArgumentNullException.ThrowIfNull(plan);
    if (_revision == long.MaxValue || plan.State.Npc.Revision == long.MaxValue ||
        _commitVersion.Value == long.MaxValue) {
      return OwnerCommitResult.Rejected(
          ProtocolErrorCode.NotReady, "NPC damage commit version range is exhausted.");
    }
    return Validate(plan.State);
  }

  public (OwnerCommitResult Result, NpcView? Npc) Commit(NpcDamagePlan plan) {
    ArgumentNullException.ThrowIfNull(plan);
    OwnerCommitResult validation = Validate(plan);
    if (validation.Status != OwnerCommitStatus.Applied) {
      return (validation, null);
    }
    var result = Commit(plan.State);
    if (result.Result.Status == OwnerCommitStatus.Applied) {
      plan.State.Npc.RecordHit(plan.Request.InteractionEpoch);
    }
    return result;
  }
}
