using System.Numerics;
using MinimalImplementation.Contracts;

namespace MinimalImplementation.Combat.Npcs;

public sealed partial class NpcManager {
  /// <summary>
  /// NPC.AI_007_TownEntities ordinary homeless Guide idle/walk/flee subset.
  /// Housing, doors, conversations and ranged attacks require separate owners and are not simulated.
  /// </summary>
  public NpcPreparation<NpcAiPlan> PrepareGuideTick(
      NpcGuideTickInput input, INpcTickMotionSnapshot motion, INpcAiRandom random) {
    ArgumentNullException.ThrowIfNull(input);
    ArgumentNullException.ThrowIfNull(motion);
    ArgumentNullException.ThrowIfNull(random);
    if (input.SourceWorldEpoch != motion.WorldEpoch ||
        input.SourceWorldCommitVersion != motion.WorldCommitVersion) {
      return NpcPreparation<NpcAiPlan>.Rejected(
          ProtocolErrorCode.Conflict, "Guide motion and AI facts need the same world revision.");
    }
    if (!TryGet(input.Entity, out NpcView? view) || view is null || view.Life <= 0 ||
        view.Revision != input.SourceNpcRevision) {
      return NpcPreparation<NpcAiPlan>.Rejected(
          ProtocolErrorCode.StaleEntity, "Guide AI needs a living current NPC revision.");
    }
    Npc npc = _byIdentity[input.Entity];
    if (view.Type != 22 || !npc.Home.Homeless || view.Ai[0] is not (0 or 1)) {
      return NpcPreparation<NpcAiPlan>.Rejected(
          ProtocolErrorCode.NotReady, "Only homeless Guide idle/walk AI is supported.");
    }
    long ownerRevision = _revision;
    float[] ai = view.Ai.ToArray();
    Vector2 velocity = view.Velocity;
    bool right = view.DirectionPositive;
    int direction = right ? 1 : -1;
    int tileX = (int)((view.Position.X + view.Width / 2 + 15 * direction) / 16);
    int footY = (int)((view.Position.Y + view.Height) / 16);
    bool groundAhead = false;
    for (int offset = 0; offset < 6; offset++) {
      NpcFighterTile tile = motion.GetTile(tileX, footY + offset);
      groundAhead |= tile.IsActive && (tile.IsSolid || tile.IsSolidTop);
    }
    if (input.ThreatPosition is Vector2 threat &&
        Vector2.DistanceSquared(threat, view.Position) < 300 * 300) {
      right = threat.X < view.Position.X;
      ai[0] = 1;
      ai[1] = Math.Max(ai[1], 120);
    }
    if (ai[0] == 0) {
      velocity.X = Math.Abs(velocity.X) <= .1f ? 0 :
          velocity.X - Math.Sign(velocity.X) * .1f;
      ai[1] = Math.Max(0, ai[1] - 1);
      if (ai[1] == 0) {
        if (groundAhead) {
          ai[0] = 1;
          ai[1] = 200 + random.Next(0, 300);
        } else {
          right = !right;
          ai[1] = 60 + random.Next(0, 120);
        }
      }
    } else {
      ai[1]--;
      if (ai[1] <= 0) {
        ai[0] = 0;
        ai[1] = 300 + random.Next(0, 300) + random.Next(0, 900);
        ai[2] = 0;
      }
      float speed = input.ThreatPosition.HasValue ? 1.5f : 1f;
      float acceleration = input.ThreatPosition.HasValue ? .1f : .07f;
      velocity.X = Math.Clamp(
          velocity.X + (right ? acceleration : -acceleration), -speed, speed);
      if (!groundAhead && velocity.Y == 0) {
        right = !right;
        velocity.X = 0;
        ai[0] = 0;
        ai[1] = 50 + random.Next(0, 50);
      }
      if (npc.Motion.CollideX && velocity.Y == 0) {
        velocity.Y = -5;
      }
    }
    velocity.Y = Math.Min(velocity.Y + .3f, 10);
    NpcMotionResult movement = motion.Move(view, view.Position, velocity, false);
    if (_revision != ownerRevision || movement.OldVelocity is null) {
      return NpcPreparation<NpcAiPlan>.Rejected(
          ProtocolErrorCode.Conflict, "Guide inputs changed during motion/random capture.");
    }
    var update = npc.CreateStateUpdate(movement.Position, movement.Velocity, view.Target,
        right, view.DirectionYPositive, right, ai, view.Life);
    var prepared = PrepareStateUpdate(input.Entity, update);
    if (!prepared.IsPrepared || prepared.Plan is null) {
      return NpcPreparation<NpcAiPlan>.Rejected(
          prepared.Result.ErrorCode ?? ProtocolErrorCode.Conflict,
          prepared.Result.Detail ?? "Guide AI state was rejected.");
    }
    return NpcPreparation<NpcAiPlan>.Prepared(new NpcAiPlan(prepared.Plan,
        new NpcMotionState(movement.OldVelocity.Value, movement.CollideX, movement.CollideY),
        false));
  }
}
