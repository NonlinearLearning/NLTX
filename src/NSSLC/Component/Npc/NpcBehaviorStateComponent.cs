namespace Terraria.Npc;

public sealed class NpcBehaviorStateComponent
{
  public NpcBehaviorStateComponent(int behaviorKind, int legacyAiStyle, int action)
  {
    BehaviorKind = behaviorKind;
    LegacyAiStyle = legacyAiStyle;
    Action = action;
    AuthoritativeAiSlots = new float[4];
  }

  public int BehaviorKind { get; }

  public int LegacyAiStyle { get; }

  public int Action { get; set; }

  public float[] AuthoritativeAiSlots { get; }

  public long LastUpdatedTick { get; set; }

  public bool HasAuthoritativeSlots => AuthoritativeAiSlots.Length == 4;

  public void CommitAiStateSlots(in NpcAiStateComponent aiState)
  {
    if (!HasAuthoritativeSlots)
    {
      throw new InvalidOperationException("NPC AI state must contain exactly four slots.");
    }

    AuthoritativeAiSlots[0] = aiState.State0;
    AuthoritativeAiSlots[1] = aiState.State1;
    AuthoritativeAiSlots[2] = aiState.State2;
    AuthoritativeAiSlots[3] = aiState.State3;
  }
}
