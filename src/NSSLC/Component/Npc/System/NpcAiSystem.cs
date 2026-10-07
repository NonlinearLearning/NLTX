namespace Terraria.Npc;

// Finite simulation rules for seven explicit net ids; reference AI coverage is tracked separately.
public sealed class NpcAiSystem {
  private readonly IReadOnlyList<INpcAiBehavior> _behaviors;

  public NpcAiSystem() {
    _behaviors =
    [
      new NpcGuideAiBehavior(),
      new NpcSlimeAiBehavior(),
      new NpcFloatingEyeAiBehavior(),
      new NpcFighterAiBehavior(),
      new NpcFallbackAiBehavior(),
    ];
  }

  public NpcAiDecision Evaluate(in NpcAiInput input) {
    foreach (INpcAiBehavior behavior in _behaviors) {
      if (behavior.CanHandle(in input)) {
        return behavior.Evaluate(in input);
      }
    }

    throw new InvalidOperationException(
      $"No NPC AI behavior is registered for net id {input.Definition.NetId}.");
  }

  public NpcAiDecision EvaluateAndCommitFinite(
      in NpcAiInput input,
      NpcBehaviorStateComponent behaviorState,
      NpcLocalBehaviorStateComponent localBehaviorState) {
    ArgumentNullException.ThrowIfNull(behaviorState);
    ArgumentNullException.ThrowIfNull(localBehaviorState);
    if (!behaviorState.HasAuthoritativeSlots) {
      throw new InvalidOperationException(
        "Finite NPC AI state must contain exactly four authoritative slots.");
    }
    if (localBehaviorState.LocalAiSlots.Length != NpcLocalBehaviorStateComponent.LocalAiSlotCount) {
      throw new InvalidOperationException(
        "Finite NPC local AI state must contain exactly four authoritative slots.");
    }

    NpcAiDecision decision = Evaluate(in input);
    behaviorState.Action = decision.Action;
    behaviorState.CommitAiStateSlots(decision.State);
    localBehaviorState.LocalAiSlots[0] = decision.State.LocalAi0;
    localBehaviorState.LocalAiSlots[1] = decision.State.LocalAi1;
    localBehaviorState.LocalAiSlots[2] = decision.State.LocalAi2;
    localBehaviorState.LocalAiSlots[3] = decision.State.LocalAi3;
    return decision;
  }
}
