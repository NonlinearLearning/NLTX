using System;
using System.Numerics;

namespace Terraria.Ai.Examples;

/// <summary>SlimeDomain bound through the common AI layer; emits jump requests only.</summary>
public sealed class SlimeAiBehavior : HtnAiBehavior<SlimeContext, ExampleAiIntent> {
  private static readonly AiRequirements _requirements = AiRequirements.Empty
      .Require<ExampleMovementComponent>()
      .Require<ExampleTargetComponent>();

  private int _targetRevision;

  public SlimeAiBehavior() : base(_requirements, SlimeDomain.Create(), new SlimeContext()) {
  }

  protected override void ClearIntents() {
    _context.ClearIntent();
  }

  protected override void ReadSnapshot(AiSnapshot snapshot, float deltaTime) {
    ExampleMovementComponent movement = snapshot.Get<ExampleMovementComponent>();
    ExampleTargetComponent target = snapshot.Get<ExampleTargetComponent>();
    bool hasTarget = target.IsVisible && target.IsAlive;
    _targetRevision = target.Revision;
    _context.TargetDirection = hasTarget
        ? MathF.Sign(target.Position.X - movement.Position.X) : 0f;
    _context.SetState(SlimeFact.HasTarget, hasTarget);
    _context.SetState(SlimeFact.IsGrounded, movement.IsGrounded);
    _context.SetState(SlimeFact.JumpReady, movement.JumpReady);
  }

  protected override void WriteIntents(AiOutput<ExampleAiIntent> output) {
    if (_context.WantsJump) {
      output.Emit(new ExampleAiIntent(ExampleAiIntentKind.Jump,
          new Vector2(_context.MoveDirection, 0f), _targetRevision));
    }
  }
}
