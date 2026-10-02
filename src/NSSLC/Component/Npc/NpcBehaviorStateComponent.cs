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
}
