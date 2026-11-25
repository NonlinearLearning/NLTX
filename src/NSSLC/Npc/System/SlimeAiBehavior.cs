using System.Collections.Generic;
using Terraria.Share.Ai;

namespace Terraria.Unshared.Npc;

/// <summary>
/// Ordinary type-1 subset of the original AI_001_Slimes behavior.
///
/// The original slime counts grounded frames before jumping, accelerates toward its target while
/// airborne, and damps horizontal speed while resting. The spatial system applies gravity and
/// collision after this behavior emits the intent.
/// </summary>
public sealed class SlimeAiBehavior : AiBehavior<NpcAiIntent>
{
  private float _jumpTimer = -100f;
  private float _direction = 1f;
  private bool _initialized;

  /// <summary>
  /// Creates one runtime. The read contract comes from the composition-root declaration, so this
  /// behavior cannot disagree with what the catalog publishes.
  /// </summary>
  public SlimeAiBehavior(AiRequirements requirements) : base(requirements)
  {
  }

  protected override void CalculateCore(
      AiSnapshot snapshot,
      float deltaTime,
      AiOutput<NpcAiIntent> output)
  {
    NpcAiMovementFact movement = snapshot.Get<NpcAiMovementFact>();
    NpcAiTargetFact target = snapshot.Get<NpcAiTargetFact>();
    NpcAiLifeFact life = snapshot.Get<NpcAiLifeFact>();
    NpcAiWorldFact world = snapshot.Get<NpcAiWorldFact>();
    IReadOnlyList<NpcAiIntent> intents = SlimeAiRules.Calculate(
        movement,
        target,
        life,
        world,
        _jumpTimer,
        _initialized,
        _direction,
        deltaTime);
    NpcAiIntent state = intents[0];
    _jumpTimer = state.StateSlot1;
    _direction = state.StateSlot0;
    _initialized = state.ActionPhase > 0;
    foreach (NpcAiIntent intent in intents)
    {
      output.Emit(intent);
    }
  }

  protected override void StopCore(AiOutput<NpcAiIntent> output)
  {
    _jumpTimer = -100f;
    _direction = 1f;
    _initialized = false;
    output.Emit(new NpcAiIntent(NpcAiIntentKind.StopMovement));
  }
}
