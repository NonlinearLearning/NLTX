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

  internal void ApplyAiState(in NpcAiStateComponent aiState)
  {
    if (AiSlots.Length != AiSlotCount)
    {
      throw new InvalidOperationException(
        "NPC AI state must contain exactly four slots.");
    }

    AiSlots[0] = aiState.State0;
    AiSlots[1] = aiState.State1;
    AiSlots[2] = aiState.State2;
    AiSlots[3] = aiState.State3;
  }
}
