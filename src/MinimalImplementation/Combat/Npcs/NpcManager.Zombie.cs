using System.Numerics;
using MinimalImplementation.Contracts;

namespace MinimalImplementation.Combat.Npcs;

public sealed partial class NpcManager {
  // NPC.AI_003_Fighters, ordinary type 3 at scale 1, v1.4.5.6/319.
  public NpcPreparation<NpcAiPlan> PrepareZombieTick(
      NpcZombieTickRequest request, INpcFighterMotionQuery motion) {
    ArgumentNullException.ThrowIfNull(request);
    ArgumentNullException.ThrowIfNull(motion);
    if (!TryGet(request.Entity, out NpcView? view) || view is null ||
        view.Revision != request.ExpectedRevision || view.Life <= 0) {
      return NpcPreparation<NpcAiPlan>.Rejected(
          ProtocolErrorCode.StaleEntity, "Zombie AI requires a current living NPC revision.");
    }
    NpcZombieEnvironment environment = request.Environment;
    if (view.Type != 3 || view.AiStyle != 3 || view.Width != 18 || view.Height != 40 ||
        environment.UsesSpecialWorldRules) {
      return NpcPreparation<NpcAiPlan>.Rejected(
          ProtocolErrorCode.NotReady, "Only ordinary, unscaled type-3 Zombie AI is migrated.");
    }
    if (!ValidPlayers(request.Players) || !double.IsFinite(environment.WorldSurfaceTiles) ||
        environment.WorldSurfaceTiles <= 0d || environment.MaximumTilesX <= 0 ||
        environment.MaximumTilesY <= 0 ||
        (!environment.WasWet && (environment.WasHoneyWet || environment.WasShimmerWet))) {
      return NpcPreparation<NpcAiPlan>.Rejected(
          ProtocolErrorCode.InvalidLength, "Zombie AI received invalid world/player facts.");
    }
    NpcTargetPlayer? priorTarget = request.Players.FirstOrDefault(p => p.Slot == view.Target);
    if (priorTarget is null) {
      return NpcPreparation<NpcAiPlan>.Rejected(
          ProtocolErrorCode.NotReady, "Zombie AI needs the prior target, including sentinel 255.");
    }
    // Include the overhead, ahead, below-foot, and six-tile expert platform probes.
    if (view.Position.X < 48f || view.Position.Y < 96f ||
        view.Position.X + view.Width + 48d >= environment.MaximumTilesX * 16d ||
        view.Position.Y + view.Height + 32d >= environment.MaximumTilesY * 16d) {
      return NpcPreparation<NpcAiPlan>.Rejected(
          ProtocolErrorCode.NotReady, "Zombie terrain probes require the captured world interior.");
    }

    Npc npc = _byIdentity[request.Entity];
    long ownerRevision = _revision;
    Vector2 position = view.Position;
    Vector2 velocity = view.Velocity;
    float[] ai = view.Ai.ToArray();
    ushort target = view.Target;
    bool right = view.DirectionPositive;
    bool down = view.DirectionYPositive;
    bool spriteRight = view.SpriteDirectionPositive;
    bool stopped = velocity.X == 0f && !npc.JustHit;
    if (priorTarget.Position.Y + priorTarget.Height == position.Y + view.Height) {
      down = false;
    }

    bool turning = velocity.Y == 0f &&
        ((velocity.X > 0f && !right) || (velocity.X < 0f && right));
    if (position.X == npc.Motion.OldPosition.X || ai[3] >= 60f || turning) {
      ai[3] += 1f;
    } else if ((double)Math.Abs(velocity.X) > 0.9 && ai[3] > 0f) {
      ai[3] -= 1f;
    }
    if (ai[3] > 600f || npc.JustHit) {
      ai[3] = 0f;
    }
    if (ZombieHitboxesIntersect(view, priorTarget)) {
      ai[3] = 0f;
    }

    bool notDiscouraged = environment.Eclipse || !environment.IsDay || view.SpawnedFromStatue ||
        position.Y > environment.WorldSurfaceTiles * 16d || priorTarget.InGraveyard;
    bool despawn = false;
    NpcTargetPlayer selectedTarget = priorTarget;
    if (ai[3] < 60f && notDiscouraged) {
      // The random moan here is a sound-only effect; it has no migrated server state.
      (target, right, down) = NpcTargetingRules.Select(
          view, request.Players, request.Confused, target, right, down);
      selectedTarget = request.Players.First(p => p.Slot == target);
      if (down && selectedTarget.Position.Y + selectedTarget.Height / 2f <=
          position.Y + view.Height) {
        down = false;
      }
    } else {
      despawn = environment.IsDay && position.Y / 16f < environment.WorldSurfaceTiles;
      if (velocity.X == 0f) {
        if (velocity.Y == 0f) {
          ai[0] += 1f;
          if (ai[0] >= 2f) {
            right = !right;
            spriteRight = right;
            ai[0] = 0f;
          }
        }
      } else {
        ai[0] = 0f;
      }
    }

    if (velocity.X < -1f || velocity.X > 1f) {
      if (velocity.Y == 0f) {
        velocity *= 0.8f;
      }
    } else if (velocity.X < 1f && right) {
      velocity.X = Math.Min(velocity.X + 0.07f, 1f);
    } else if (velocity.X > -1f && !right) {
      velocity.X = Math.Max(velocity.X - 0.07f, -1f);
    }

    bool supported = false;
    if (velocity.Y == 0f) {
      int floorY = (int)(position.Y + view.Height + 7f) / 16;
      int headY = (int)(position.Y - 9f) / 16;
      int left = (int)(position.X + 8f) / 16;
      int last = (int)(position.X + view.Width - 8f) / 16;
      for (int x = left; x <= last; x++) {
        if (motion.GetTile(x, headY).SolidWithoutPlatforms) {
          supported = false;
          break;
        }
        if (motion.GetTile(x, floorY).SolidAllowBottomSlope) {
          supported = true;
        }
      }
    }
    if (velocity.Y >= 0f) {
      position = StepZombie(view, position, velocity, environment, motion);
    }
    if (supported) {
      int direction = right ? 1 : -1;
      int x = (int)((position.X + view.Width / 2 + 15 * direction) / 16f);
      int y = (int)((position.Y + view.Height - 15f) / 16f);
      NpcFighterTile ahead = motion.GetTile(x, y);
      NpcFighterTile above = motion.GetTile(x, y - 1);
      if (above.IsActive && above.IsClosedDoorOrGate) {
        return NpcPreparation<NpcAiPlan>.Rejected(
            ProtocolErrorCode.NotReady, "Zombie door/gate actions require an unmigrated world effect.");
      }
      if ((velocity.X < 0f && !spriteRight) || (velocity.X > 0f && spriteRight)) {
        if (motion.GetTile(x, y - 2).SolidWithoutPlatforms) {
          velocity.Y = motion.GetTile(x, y - 3).SolidWithoutPlatforms ? -8f : -7f;
        } else if (above.SolidWithoutPlatforms) {
          velocity.Y = -6f;
        } else if (position.Y + view.Height - y * 16f > 20f &&
            !ahead.IsTopSlope && ahead.SolidWithoutPlatforms) {
          velocity.Y = -5f;
        } else if (!down && !motion.GetTile(x, y + 1).SolidAllowBottomSlope &&
            !motion.GetTile(x + direction, y + 1).SolidAllowBottomSlope) {
          velocity.Y = -8f;
          velocity.X *= 1.5f;
        } else {
          ai[1] = 0f;
          ai[2] = 0f;
        }
        if (velocity.Y == 0f && stopped && ai[3] == 1f) {
          velocity.Y = -5f;
        }
        float targetBottom = selectedTarget.Position.Y + selectedTarget.Height;
        if (velocity.Y == 0f && environment.ExpertMode && targetBottom < position.Y &&
            Math.Abs(position.X + view.Width / 2f -
                (selectedTarget.Position.X + selectedTarget.Width / 2f)) <
                selectedTarget.Width * 3f && motion.CanHitTarget(view, position, selectedTarget)) {
          if (targetBottom > position.Y - 96f) {
            velocity.Y = -7.9f;
          } else {
            int centerX = (int)((position.X + view.Width / 2) / 16f);
            int bottomY = (int)((position.Y + view.Height) / 16f) - 1;
            for (int platformY = bottomY; platformY > bottomY - 6; platformY--) {
              NpcFighterTile tile = motion.GetTile(centerX, platformY);
              if (tile.IsActive && tile.IsPlatform) {
                velocity.Y = -7.9f;
                break;
              }
            }
          }
        }
      }
    } else {
      ai[1] = 0f;
      ai[2] = 0f;
    }

    (float gravity, float maximumFallSpeed) = NpcGravityRules.Calculate(
        position.Y, environment.WorldSurfaceTiles, environment.MaximumTilesX,
        environment.WasWet, environment.WasHoneyWet, environment.WasShimmerWet);
    velocity.Y = Math.Min(velocity.Y + gravity, maximumFallSpeed);
    if ((double)velocity.X < 0.005 && (double)velocity.X > -0.005) {
      velocity.X = 0f;
    }
    NpcMotionResult movement = motion.Move(view, position, velocity, fallThroughPlatforms: down);
    ArgumentNullException.ThrowIfNull(movement);
    if (_revision != ownerRevision) {
      return NpcPreparation<NpcAiPlan>.Rejected(
          ProtocolErrorCode.Conflict, "The NPC owner changed during fighter motion queries.");
    }
    if (movement.OldPosition is not { } oldPosition ||
        !float.IsFinite(oldPosition.X) || !float.IsFinite(oldPosition.Y)) {
      return NpcPreparation<NpcAiPlan>.Rejected(
          ProtocolErrorCode.NotReady, "Zombie movement requires its real pre-displacement position.");
    }
    if (movement.OldVelocity is not { } oldVelocity ||
        !float.IsFinite(oldVelocity.X) || !float.IsFinite(oldVelocity.Y)) {
      return NpcPreparation<NpcAiPlan>.Rejected(
          ProtocolErrorCode.NotReady, "Zombie movement requires its real pre-collision velocity.");
    }
    // FindFrame's type-3 facing affects the next tick's jump eligibility.
    if (movement.Velocity.Y == 0f) {
      spriteRight = right;
    }
    NpcStateUpdate update = npc.CreateStateUpdate(
        movement.Position, movement.Velocity, target, right, down, spriteRight, ai, view.Life);
    NpcPreparation<NpcStatePlan> state = PrepareStateUpdate(request.Entity, update);
    if (!state.IsPrepared || state.Plan is null) {
      return NpcPreparation<NpcAiPlan>.Rejected(
          state.Result.ErrorCode ?? ProtocolErrorCode.Conflict,
          state.Result.Detail ?? "Zombie AI state could not be prepared.");
    }
    return NpcPreparation<NpcAiPlan>.Prepared(new NpcAiPlan(state.Plan,
        new NpcMotionState(oldVelocity, movement.CollideX, movement.CollideY, oldPosition), despawn));
  }

  private static Vector2 StepZombie(
      NpcView npc, Vector2 position, Vector2 velocity, NpcZombieEnvironment environment,
      INpcFighterMotionQuery motion) {
    int sign = Math.Sign(velocity.X);
    float nextX = position.X + velocity.X;
    int x = (int)((nextX + npc.Width / 2 + (npc.Width / 2 + 1) * sign) / 16f);
    int y = (int)((position.Y + npc.Height - 1f) / 16f);
    if (x < 4 || y < 4 || x >= environment.MaximumTilesX - 4 ||
        y >= environment.MaximumTilesY - 4) {
      return position;
    }
    NpcFighterTile tile = motion.GetTile(x, y);
    NpcFighterTile above = motion.GetTile(x, y - 1);
    bool stepSurface = (tile.BlocksStep && !tile.IsTopSlope && !above.IsTopSlope) ||
        (above.IsHalfBlock && above.IsActive);
    bool clearAbove = !above.BlocksStep ||
        (above.IsHalfBlock && !motion.GetTile(x, y - 4).BlocksStep);
    NpcFighterTile behind = motion.GetTile(x - sign, y - 3);
    if (x * 16f >= nextX + npc.Width || x * 16f + 16f <= nextX || !stepSurface ||
        !clearAbove || motion.GetTile(x, y - 2).BlocksStep ||
        motion.GetTile(x, y - 3).BlocksStep || (behind.IsActive && behind.IsSolid)) {
      return position;
    }
    float stepTop = y * 16f;
    if (tile.IsHalfBlock) {
      stepTop += 8f;
    }
    if (above.IsHalfBlock) {
      stepTop -= 8f;
    }
    float rise = position.Y + npc.Height - stepTop;
    if (rise > 0f && rise <= 16.1f) {
      position.Y = stepTop - npc.Height;
    }
    return position;
  }

  private static bool ZombieHitboxesIntersect(NpcView npc, NpcTargetPlayer player) {
    int x = (int)npc.Position.X;
    int y = (int)npc.Position.Y;
    int playerX = (int)player.Position.X;
    int playerY = (int)player.Position.Y;
    return x < playerX + player.Width && x + npc.Width > playerX &&
        y < playerY + player.Height && y + npc.Height > playerY;
  }
}
