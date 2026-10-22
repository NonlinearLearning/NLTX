using System.Numerics;
using MinimalImplementation.Combat.Identity;
using MinimalImplementation.Contracts;

namespace MinimalImplementation.Combat.Npcs;

public sealed partial class NpcManager {
  private const int ActivityRangeX = 4032;
  private const int ActivityRangeY = 2520;
  private const int ActivityScreenWidth = 1920;
  private const int ActivityScreenHeight = 1200;

  /// <summary>
  /// Source NPC.CheckActive for ordinary types 1/2/3. Supply all player slots from
  /// one P02 snapshot, including active dead/ghost players; an empty world is valid.
  /// </summary>
  public NpcPreparation<NpcActivityPlan> PrepareActivity(
      CombatEntityIdentity identity, long expectedRevision,
      IReadOnlyList<NpcTargetPlayer> players) {
    ArgumentNullException.ThrowIfNull(players);
    if (!TryGet(identity, out NpcView? view) || view is null ||
        view.Revision != expectedRevision || view.Life <= 0) {
      return NpcPreparation<NpcActivityPlan>.Rejected(
          ProtocolErrorCode.StaleEntity, "NPC activity needs a current living generation.");
    }
    if (view.Type is not (1 or 2 or 3) || view.IsBoss || view.IsTownNpc) {
      return NpcPreparation<NpcActivityPlan>.Rejected(
          ProtocolErrorCode.NotReady, "This NPC's activity exemptions are not migrated.");
    }
    if (!ValidPlayers(players, requireFallback: false) ||
        !HasActivityBounds(view.Position, view.Width, view.Height) ||
        players.Any(player => !HasActivityBounds(player.Position, player.Width, player.Height))) {
      return NpcPreparation<NpcActivityPlan>.Rejected(
          ProtocolErrorCode.InvalidLength, "NPC activity received invalid player/bounds facts.");
    }

    Npc npc = _byIdentity[identity];
    float centerX = view.Position.X + view.Width / 2;
    float centerY = view.Position.Y + view.Height / 2;
    int outerX = (int)(centerX - ActivityRangeX);
    int outerY = (int)(centerY - ActivityRangeY);
    int innerX = (int)((double)centerX - ActivityScreenWidth * 0.5 - view.Width);
    int innerY = (int)((double)centerY - ActivityScreenHeight * 0.5 - view.Height);
    int timeLeft = npc.ActivityTimeLeft;
    var nearby = new List<byte>();
    foreach (NpcTargetPlayer player in players.OrderBy(player => player.Slot)) {
      if (!player.Active || player.Slot == byte.MaxValue) {
        continue;
      }
      if (IntersectsPlayer(outerX, outerY, ActivityRangeX * 2, ActivityRangeY * 2, player)) {
        nearby.Add(player.Slot);
      }
      if (IntersectsPlayer(innerX, innerY, ActivityScreenWidth + view.Width * 2,
          ActivityScreenHeight + view.Height * 2, player)) {
        timeLeft = Npc.InitialActivityTime;
      }
    }
    timeLeft = Math.Max(0, timeLeft - 1);
    bool despawn = nearby.Count == 0 || timeLeft == 0;
    NpcStateUpdate update = npc.CreateStateUpdate(
        view.Position, view.Velocity, view.Target, view.DirectionPositive,
        view.DirectionYPositive, view.SpriteDirectionPositive, view.Ai, despawn ? 0 : view.Life);
    NpcPreparation<NpcStatePlan> state = PrepareStateUpdate(identity, update);
    if (!state.IsPrepared || state.Plan is null) {
      return NpcPreparation<NpcActivityPlan>.Rejected(
          state.Result.ErrorCode ?? ProtocolErrorCode.Conflict,
          state.Result.Detail ?? "NPC activity state could not be prepared.");
    }
    return NpcPreparation<NpcActivityPlan>.Prepared(
        new NpcActivityPlan(state.Plan, timeLeft, despawn, nearby));
  }

  public OwnerCommitResult Validate(NpcActivityPlan plan) {
    ArgumentNullException.ThrowIfNull(plan);
    OwnerCommitResult validation = Validate(plan.State);
    if (validation.Status != OwnerCommitStatus.Applied) {
      return validation;
    }
    if (_revision == long.MaxValue || plan.State.Npc.Revision == long.MaxValue ||
        _commitVersion.Value == long.MaxValue) {
      return OwnerCommitResult.Rejected(
          ProtocolErrorCode.NotReady, "NPC activity commit version range is exhausted.");
    }
    return validation;
  }

  public NpcActivityCommitResult Commit(NpcActivityPlan plan) {
    ArgumentNullException.ThrowIfNull(plan);
    OwnerCommitResult validation = Validate(plan);
    if (validation.Status != OwnerCommitStatus.Applied) {
      return new NpcActivityCommitResult(validation, null, false, Array.Empty<byte>());
    }
    Npc npc = plan.State.Npc;
    CommitVersion nextVersion = _commitVersion.Next();
    // Authority failure must not consume the timer or turn a living NPC into a death.
    if (plan.Despawn && !_identityAuthority.Retire(npc.Entity)) {
      return new NpcActivityCommitResult(OwnerCommitResult.Rejected(
          ProtocolErrorCode.StaleEntity, "The identity authority rejected NPC despawn."),
          null, false, Array.Empty<byte>());
    }
    npc.ApplyActivity(plan.TimeLeft, plan.Despawn, nextVersion);
    NpcView snapshot = npc.Capture();
    if (plan.Despawn) {
      npc.Retire();
      _slots[npc.Slot] = null;
      _byIdentity.Remove(npc.Entity);
      _coreLoot.Remove(npc.Entity);
    }
    CommitMutation(nextVersion);
    return new NpcActivityCommitResult(
        OwnerCommitResult.Applied(nextVersion), snapshot, plan.Despawn, plan.NearbyPlayerSlots);
  }

  private static bool HasActivityBounds(Vector2 position, int width, int height) {
    // Leave room for the source integer rectangles and their far edges.
    return (double)position.X >= int.MinValue + ActivityRangeX * 2d &&
        (double)position.Y >= int.MinValue + ActivityRangeY * 2d &&
        (double)position.X + width <= int.MaxValue - ActivityRangeX * 2d &&
        (double)position.Y + height <= int.MaxValue - ActivityRangeY * 2d;
  }

  private static bool IntersectsPlayer(
      int x, int y, int width, int height, NpcTargetPlayer player) {
    int playerX = (int)player.Position.X;
    int playerY = (int)player.Position.Y;
    return x < (long)playerX + player.Width && playerX < (long)x + width &&
        y < (long)playerY + player.Height && playerY < (long)y + height;
  }
}
