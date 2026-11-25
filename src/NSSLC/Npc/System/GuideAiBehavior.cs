using System;
using System.Numerics;
using Terraria.Share.Ai;

namespace Terraria.Unshared.Npc;

/// <summary>
/// Homeless Guide subset of the original AI_007_TownEntities behavior.
///
/// The ECS movement system owns gravity and collision. This behavior supplies the same
/// observable idle/walk intent, acceleration target and the small jump used to recover from a
/// horizontal obstruction.
/// </summary>
public sealed class GuideAiBehavior : AiBehavior<NpcAiIntent>
{
  private const float SimulationTicksPerSecond = 60f;

  private float _modeTime;
  private int _direction = 1;
  private bool _walking;
  private bool _blockedLastTick;

  /// <summary>
  /// Creates one runtime. The read contract comes from the composition-root declaration, so this
  /// behavior cannot disagree with what the catalog publishes.
  /// </summary>
  public GuideAiBehavior(AiRequirements requirements) : base(requirements)
  {
  }

  protected override void CalculateCore(
      AiSnapshot snapshot,
      float deltaTime,
      AiOutput<NpcAiIntent> output)
  {
    NpcAiMovementFact movement = snapshot.Get<NpcAiMovementFact>();
    _modeTime += deltaTime;
    if (_modeTime >= (_walking ? 4.0f : 4.0f))
    {
      _modeTime = 0f;
      _walking = !_walking;
      if (_walking)
      {
        _direction = -_direction;
      }
    }

    NpcBehaviorPhase phase = _walking ? NpcBehaviorPhase.Wander : NpcBehaviorPhase.Idle;
    bool blocked = _walking && movement.IsGrounded && _modeTime > 0.25f &&
        MathF.Abs(movement.Velocity.X) < 0.05f;
    bool jumpRequested = blocked && !_blockedLastTick;
    if (jumpRequested)
    {
      _direction = -_direction;
    }
    _blockedLastTick = blocked;
    Vector2 desired = _walking
        ? new Vector2(_direction * SimulationTicksPerSecond, 0f)
        : Vector2.Zero;
    output.Emit(new NpcAiIntent(
        NpcAiIntentKind.SetBehaviorState,
        phase,
        desiredVelocity: desired,
        jumpRequested: jumpRequested,
        stateSlot0: _direction,
        stateSlot1: _modeTime));
    output.Emit(new NpcAiIntent(
        NpcAiIntentKind.Move,
        phase,
        desiredVelocity: desired,
        jumpRequested: jumpRequested,
        stateSlot0: _direction,
        stateSlot1: _modeTime));
  }

  protected override void StopCore(AiOutput<NpcAiIntent> output)
  {
    output.Emit(new NpcAiIntent(NpcAiIntentKind.StopMovement));
  }
}
