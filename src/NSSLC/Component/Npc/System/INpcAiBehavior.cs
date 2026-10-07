namespace Terraria.Npc;

public interface INpcAiBehavior {
  bool CanHandle(in NpcAiInput input);

  NpcAiDecision Evaluate(in NpcAiInput input);
}
