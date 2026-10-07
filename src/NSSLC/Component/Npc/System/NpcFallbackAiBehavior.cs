namespace Terraria.Npc;

internal sealed class NpcFallbackAiBehavior : INpcAiBehavior {
  public bool CanHandle(in NpcAiInput input) {
    return input.Definition.NetId is 37 or 488;
  }

  public NpcAiDecision Evaluate(in NpcAiInput input) {
    if (input.Definition.Town.IsTownNpc || input.Definition.NetId == 488) {
      return NpcAiBehaviorMath.KeepCurrent(input, skipMovement: true);
    }

    return NpcAiBehaviorMath.KeepCurrent(input);
  }
}
