using System.Numerics;
using MinimalImplementation.Contracts;

namespace MinimalImplementation.Combat.Npcs;

public sealed partial class NpcManager {
  public NpcPreparation<NpcAiPlan> PrepareEyeTick(
      NpcEyeTickRequest request, INpcMotionQuery motion) {
    ArgumentNullException.ThrowIfNull(request);
    ArgumentNullException.ThrowIfNull(motion);
    if (!TryGet(request.Entity, out NpcView? view) || view is null ||
        view.Revision != request.ExpectedRevision || view.Life <= 0) {
      return NpcPreparation<NpcAiPlan>.Rejected(
          ProtocolErrorCode.StaleEntity, "NPC AI requires a current living generation and revision.");
    }
    if (view.Type != 2 || view.AiStyle != 2) {
      return NpcPreparation<NpcAiPlan>.Rejected(
          ProtocolErrorCode.NotReady, "Only the ordinary type-2 eye AI is migrated here.");
    }
    if (!ValidPlayers(request.Players) || !double.IsFinite(request.WorldSurfaceTiles)) {
      return NpcPreparation<NpcAiPlan>.Rejected(
          ProtocolErrorCode.InvalidLength, "NPC AI received invalid world/player facts.");
    }

    Npc npc = _byIdentity[request.Entity];
    long ownerRevision = _revision;
    Vector2 velocity = NpcEyeAiRules.Bounce(view.Velocity, npc.Motion, view.DirectionPositive);
    ushort target = view.Target;
    bool right = view.DirectionPositive;
    bool down = view.DirectionYPositive;
    NpcTargetPlayer? oldTarget = request.Players.FirstOrDefault(player => player.Slot == target);
    if (oldTarget is null) {
      return NpcPreparation<NpcAiPlan>.Rejected(
          ProtocolErrorCode.NotReady, "Eye AI needs the prior target's graveyard facts.");
    }
    bool despawn = !oldTarget.InGraveyard && request.IsDay &&
        view.Position.Y <= request.WorldSurfaceTiles * 16d;
    if (despawn) {
      down = false;
      right = velocity.X > 0f;
    } else {
      (target, right, down) = NpcTargetingRules.Select(
          view, request.Players, request.Confused, target, right, down);
    }
    bool wet = motion.IsWet(view);
    velocity = NpcEyeAiRules.Steer(velocity, right, down, wet);
    if (wet) {
      (target, right, down) = NpcTargetingRules.Select(
          view, request.Players, request.Confused, target, right, down);
    }
    NpcMotionResult movement = motion.Move(view, velocity);
    ArgumentNullException.ThrowIfNull(movement);
    if (_revision != ownerRevision) {
      return NpcPreparation<NpcAiPlan>.Rejected(
          ProtocolErrorCode.Conflict, "The NPC owner changed during motion queries.");
    }
    if (movement.OldVelocity is not { } oldVelocity ||
        !float.IsFinite(oldVelocity.X) || !float.IsFinite(oldVelocity.Y)) {
      return NpcPreparation<NpcAiPlan>.Rejected(
          ProtocolErrorCode.NotReady, "Eye movement requires its real pre-collision velocity.");
    }
    NpcStateUpdate update = npc.CreateStateUpdate(
        movement.Position, movement.Velocity, target, right, down,
        view.SpriteDirectionPositive, view.Ai, view.Life);
    NpcPreparation<NpcStatePlan> state = PrepareStateUpdate(request.Entity, update);
    if (!state.IsPrepared || state.Plan is null) {
      return NpcPreparation<NpcAiPlan>.Rejected(
          state.Result.ErrorCode ?? ProtocolErrorCode.Conflict,
          state.Result.Detail ?? "NPC AI state could not be prepared.");
    }
    return NpcPreparation<NpcAiPlan>.Prepared(new NpcAiPlan(state.Plan,
        new NpcMotionState(oldVelocity, movement.CollideX, movement.CollideY), despawn));
  }

  public OwnerCommitResult Validate(NpcAiPlan plan) {
    ArgumentNullException.ThrowIfNull(plan);
    if (_revision == long.MaxValue || plan.State.Npc.Revision == long.MaxValue ||
        _commitVersion.Value == long.MaxValue) {
      return OwnerCommitResult.Rejected(
          ProtocolErrorCode.NotReady, "NPC AI commit version range is exhausted.");
    }
    return Validate(plan.State);
  }

  public (OwnerCommitResult Result, NpcView? Npc) Commit(NpcAiPlan plan) {
    ArgumentNullException.ThrowIfNull(plan);
    OwnerCommitResult validation = Validate(plan);
    if (validation.Status != OwnerCommitStatus.Applied) {
      return (validation, null);
    }
    var committed = Commit(plan.State);
    if (committed.Result.Status == OwnerCommitStatus.Applied) {
      plan.State.Npc.ApplyMotion(plan.Motion);
      if (plan.CombatStats is { } stats) {
        plan.State.Npc.ApplyCombatStats(stats);
      }
      if (plan.EncourageDespawn) {
        plan.State.Npc.EncourageDespawn();
      }
      return (committed.Result, plan.State.Npc.Capture());
    }
    return committed;
  }

  private static bool ValidPlayers(
      IReadOnlyList<NpcTargetPlayer> players, bool requireFallback = true) {
    var slots = new HashSet<byte>();
    foreach (NpcTargetPlayer player in players) {
      if (player is null || !slots.Add(player.Slot) ||
          !FiniteBounds(player.Position, player.Width, player.Height)) {
        return false;
      }
      if (player.TankPet is { } pet && !FiniteBounds(pet.Position, pet.Width, pet.Height)) {
        return false;
      }
    }
    // Source targeting may fall back to slot zero when no active player exists.
    return !requireFallback || slots.Contains(0);
  }

  private static bool FiniteBounds(Vector2 position, int width, int height) {
    return float.IsFinite(position.X) && float.IsFinite(position.Y) && width > 0 && height > 0;
  }
}
