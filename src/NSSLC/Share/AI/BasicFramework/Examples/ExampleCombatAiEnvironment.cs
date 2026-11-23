using System;
using System.Numerics;
using Terraria.Ai.Goap.Core;

namespace Terraria.Ai.Examples;

/// <summary>
/// GOAP environment that only reads copied inputs and emits ExampleAiIntent values.
/// </summary>
public sealed class ExampleCombatAiEnvironment
    : GoapAiEnvironment<ExampleAiIntent>, ICombatAiEnvironment {
  private sealed class SnapshotTarget : ITarget {
    public Vector3 Position { get; set; }
    public bool Valid { get; set; }
    public int Revision { get; }

    public SnapshotTarget(int revision) {
      Revision = revision;
    }

    public bool IsValid() => Valid;
  }

  private ExampleMovementComponent _movement;
  private ExampleAttackFeedbackComponent _feedback;
  private SnapshotTarget? _target;
  private long _nextRequestId;
  private long _attackRequestId;
  private int _attackTargetRevision;

  public override Vector3 Position => new(_movement.Position, 0f);

  public override ITarget? GetTarget(IGoapAction action) {
    return _target?.IsValid() == true ? _target : null;
  }

  public override void Move(ITarget target) {
    SnapshotTarget observation = GetObservation(target);
    _output.Emit(new ExampleAiIntent(ExampleAiIntentKind.MoveTo,
        new Vector2(target.Position.X, target.Position.Y), observation.Revision));
  }

  public override void StopMoving() {
    _output.Emit(new ExampleAiIntent(ExampleAiIntentKind.StopMoving, default));
  }

  public void BeginAttack(ITarget target) {
    _attackTargetRevision = GetObservation(target).Revision;
    _attackRequestId = checked(++_nextRequestId);
    _output.Emit(new ExampleAiIntent(ExampleAiIntentKind.BeginAttack,
        new Vector2(target.Position.X, target.Position.Y), _attackTargetRevision,
        _attackRequestId));
  }

  public bool HasAttackFinished(ITarget target) {
    return target.IsValid() && _attackRequestId != 0 &&
        _feedback.RequestId == _attackRequestId && _feedback.Finished;
  }

  public void CancelAttack() {
    if (_attackRequestId != 0) {
      _output.Emit(new ExampleAiIntent(ExampleAiIntentKind.CancelAttack, default,
          _attackTargetRevision, _attackRequestId));
      _attackRequestId = 0;
    }
  }

  protected override void ReadSnapshot(AiSnapshot snapshot, float deltaTime) {
    _movement = snapshot.Get<ExampleMovementComponent>();
    _feedback = snapshot.Get<ExampleAttackFeedbackComponent>();
    ExampleTargetComponent target = snapshot.Get<ExampleTargetComponent>();
    if (_target == null || _target.Revision != target.Revision) {
      if (_target != null) {
        _target.Valid = false;
      }
      // Retire the old object so an in-flight action cannot silently switch to a new target.
      _target = new SnapshotTarget(target.Revision);
    }
    _target.Position = new Vector3(target.Position, 0f);
    _target.Valid = target.IsVisible && target.IsAlive;
    WorldData.SetState<EnemyAliveKey>(_target.Valid ? 1 : 0);
  }

  private static SnapshotTarget GetObservation(ITarget target) {
    return target as SnapshotTarget ??
        throw new ArgumentException("Expected a target from this snapshot environment.",
                                    nameof(target));
  }
}
