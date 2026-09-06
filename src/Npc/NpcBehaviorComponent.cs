using System;

namespace Terraria.Npc;

// status: partial
// evidenceStatus: confirmed for aiStyle/aiAction/ai[4]
// crossSubsystemOwner: integration-review
public sealed class NpcBehaviorComponent
{
  public const int AiSlotCount = 4;

  public NpcBehaviorComponent(
    int aiStyle,
    int action,
    ReadOnlySpan<float> aiSlots)
  {
    if (aiSlots.Length != AiSlotCount)
    {
      throw new ArgumentException(
        $"NPC AI state must contain exactly {AiSlotCount} slots.",
        nameof(aiSlots));
    }

    AiStyle = aiStyle;
    Action = action;
    AiSlots = aiSlots.ToArray();
  }

  public int AiStyle { get; private set; }

  public int Action { get; private set; }

  public float[] AiSlots { get; }
}
