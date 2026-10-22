using System.Numerics;
using MinimalImplementation.Contracts;

namespace MinimalImplementation.Combat.Npcs;

public sealed partial class NpcManager {
  public NpcPreparation<NpcAiPlan> PrepareSlimeTick(
      NpcSlimeTickRequest request, INpcGroundMotionQuery motion, INpcAiRandom random) {
    ArgumentNullException.ThrowIfNull(request);
    ArgumentNullException.ThrowIfNull(motion);
    ArgumentNullException.ThrowIfNull(random);
    if (!TryGet(request.Entity, out NpcView? view) || view is null ||
        view.Revision != request.ExpectedRevision || view.Life <= 0) {
      return NpcPreparation<NpcAiPlan>.Rejected(
          ProtocolErrorCode.StaleEntity, "Slime AI requires a current living NPC revision.");
    }
    NpcSlimeEnvironment environment = request.Environment;
    if (view.Type != 1 || view.AiStyle != 1 || view.Width != 24 || view.Height != 18 ||
        environment.UsesSpecialWorldRules || view.Ai[0] == -999f) {
      return NpcPreparation<NpcAiPlan>.Rejected(
          ProtocolErrorCode.NotReady, "Only ordinary, unscaled, non-balloon type-1 AI is migrated.");
    }
    if (!ValidPlayers(request.Players) || !float.IsFinite(request.Value) ||
        !double.IsFinite(environment.WorldSurfaceTiles) ||
        environment.WorldSurfaceTiles <= 0d || environment.MaximumTilesX <= 0 ||
        (!environment.WasWet && (environment.WasHoneyWet || environment.WasShimmerWet))) {
      return NpcPreparation<NpcAiPlan>.Rejected(
          ProtocolErrorCode.InvalidLength, "Slime AI received invalid world/player facts.");
    }

    Npc npc = _byIdentity[request.Entity];
    long ownerRevision = _revision;
    float[] ai = view.Ai.ToArray();
    if (ai[1] == 0f && request.Value > 0f) {
      if (npc.PreparingSlimeBody) {
        return NpcPreparation<NpcAiPlan>.Rejected(
            ProtocolErrorCode.Conflict, "Slime body initialization cannot reenter itself.");
      }
      int? bodyItem = npc.PendingSlimeBodyItem;
      if (!bodyItem.HasValue) {
        int item;
        npc.PreparingSlimeBody = true;
        try {
          item = NpcSlimeAiRules.RollBody(view, environment, random);
        } finally {
          npc.PreparingSlimeBody = false;
        }
        if (_revision != ownerRevision) {
          return NpcPreparation<NpcAiPlan>.Rejected(
              ProtocolErrorCode.Conflict, "The NPC owner changed during AI randomness.");
        }
        npc.CacheSlimeBodyItem(item);
        bodyItem = item;
      }
      ai[1] = bodyItem.Value;
    }
    if (!NpcSlimeAiRules.SupportsBody(ai[1])) {
      return NpcPreparation<NpcAiPlan>.Rejected(
          ProtocolErrorCode.NotReady,
          "This slime body item requires unmigrated world, projectile, or special-item effects.");
    }

    Vector2 position = view.Position;
    Vector2 velocity = view.Velocity;
    ushort target = view.Target;
    bool right = view.DirectionPositive;
    bool down = view.DirectionYPositive;
    (float gravity, float maximumFallSpeed) = NpcSlimeAiRules.Gravity(position.Y, environment);
    if (ai[1] == 2f && velocity.Y == 0f) {
      ai[0] += 9f;
    } else if (ai[1] == 3f && velocity.Y > 0f) {
      velocity.Y += gravity * 2f;
    } else if (ai[1] == 751f && velocity.Y != 0f) {
      velocity.Y -= gravity * 0.6f;
    } else if (ai[1] is 3736f or 3737f or 3738f &&
        npc.Motion.CollideY && npc.Motion.OldVelocity.Y > 4f) {
      velocity.Y = -npc.Motion.OldVelocity.Y * 0.7f;
    }

    bool aggressive = !environment.IsDay || view.Life != view.MaximumLife ||
        position.Y > environment.WorldSurfaceTiles * 16d || environment.SlimeRain;
    if (ai[2] > 1f) {
      ai[2] -= 1f;
    }
    if (environment.WasWet) {
      if (npc.Motion.CollideY) {
        velocity.Y = -2f;
      }
      if (velocity.Y < 0f && ai[3] == position.X) {
        right = !right;
        ai[2] = 200f;
      }
      if (velocity.Y > 0f) {
        ai[3] = position.X;
      }
      if (velocity.Y > 2f) {
        velocity.Y *= 0.9f;
      }
      velocity.Y = Math.Max(-4f, velocity.Y - 0.5f);
      if (ai[2] == 1f && aggressive) {
        (target, right, down) = NpcTargetingRules.Select(
            view, request.Players, request.Confused, target, right, down, position);
      }
    }
    if (ai[2] == 0f) {
      ai[0] = -100f;
      ai[2] = 1f;
      (target, right, down) = NpcTargetingRules.Select(
          view, request.Players, request.Confused, target, right, down, position);
    }
    if (velocity.Y == 0f) {
      if (npc.Motion.CollideY && npc.Motion.OldVelocity.Y != 0f &&
          motion.HasSolidCollision(view, position)) {
        position.X -= velocity.X + (right ? 1f : -1f);
      }
      if (ai[3] == position.X) {
        right = !right;
        ai[2] = 200f;
      }
      ai[3] = 0f;
      if (ai[1] == 3609f) {
        velocity.X = Math.Clamp(velocity.X + (right ? 0.1f : -0.1f), -2.5f, 2.5f);
      } else {
        velocity.X *= 0.8f;
        if ((double)velocity.X > -0.1 && (double)velocity.X < 0.1) {
          velocity.X = 0f;
        }
      }
      if (aggressive) {
        ai[0] += 1f;
      }
      ai[0] += 1f;
      int jump = ai[0] >= 0f ? 1 : 0;
      if (ai[0] >= -1000f && ai[0] <= -500f) {
        jump = 2;
      }
      if (ai[0] >= -2000f && ai[0] <= -1500f) {
        jump = 3;
      }
      if (jump > 0) {
        if (aggressive && ai[2] == 1f) {
          (target, right, down) = NpcTargetingRules.Select(
              view, request.Players, request.Confused, target, right, down, position);
        }
        int direction = right ? 1 : -1;
        if (jump == 3) {
          velocity.Y = -8f;
          velocity.X += 3 * direction;
          ai[0] = -200f;
          ai[3] = position.X;
        } else {
          velocity.Y = -6f;
          velocity.X += 2 * direction;
          ai[0] = jump == 1 ? -1120f : -2120f;
        }
      }
    } else if (target < 255 && (right ? velocity.X < 3f : velocity.X > -3f)) {
      int direction = right ? 1 : -1;
      if (npc.Motion.CollideX && Math.Abs(velocity.X) == 0.2f) {
        position.X -= 1.4f * direction;
      }
      if (npc.Motion.CollideY && npc.Motion.OldVelocity.Y != 0f &&
          motion.HasSolidCollision(view, position)) {
        position.X -= velocity.X + direction;
      }
      if (right ? (double)velocity.X > -0.01 : (double)velocity.X < 0.01) {
        velocity.X += 0.2f * direction;
      } else {
        velocity.X *= 0.93f;
      }
    }

    velocity.Y = Math.Min(velocity.Y + gravity, maximumFallSpeed);
    if ((double)velocity.X < 0.005 && (double)velocity.X > -0.005) {
      velocity.X = 0f;
    }
    NpcMotionResult movement = motion.Move(view, position, velocity, fallThroughPlatforms: false);
    ArgumentNullException.ThrowIfNull(movement);
    if (_revision != ownerRevision) {
      return NpcPreparation<NpcAiPlan>.Rejected(
          ProtocolErrorCode.Conflict, "The NPC owner changed during ground motion queries.");
    }
    if (movement.OldVelocity is not { } oldVelocity ||
        !float.IsFinite(oldVelocity.X) || !float.IsFinite(oldVelocity.Y)) {
      return NpcPreparation<NpcAiPlan>.Rejected(
          ProtocolErrorCode.NotReady, "Slime movement requires its real pre-collision velocity.");
    }
    NpcStateUpdate update = npc.CreateStateUpdate(
        movement.Position, movement.Velocity, target, right, down,
        view.SpriteDirectionPositive, ai, view.Life);
    NpcPreparation<NpcStatePlan> state = PrepareStateUpdate(request.Entity, update);
    if (!state.IsPrepared || state.Plan is null) {
      return NpcPreparation<NpcAiPlan>.Rejected(
          state.Result.ErrorCode ?? ProtocolErrorCode.Conflict,
          state.Result.Detail ?? "Slime AI state could not be prepared.");
    }
    return NpcPreparation<NpcAiPlan>.Prepared(new NpcAiPlan(state.Plan,
        new NpcMotionState(oldVelocity, movement.CollideX, movement.CollideY), false,
        NpcSlimeAiRules.BodyStats(npc.Definition, ai[1])));
  }
}
