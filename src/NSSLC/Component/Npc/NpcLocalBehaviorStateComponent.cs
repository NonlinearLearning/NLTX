using System;

namespace Terraria.Npc;

// status: proposed
// evidenceStatus: confirmed for Version4 localAI field; restore semantics partial
// crossSubsystemOwner: integration-review
public sealed class NpcLocalBehaviorStateComponent
{
  public const int LocalAiSlotCount = 4;

  public NpcLocalBehaviorStateComponent(ReadOnlySpan<float> localAiSlots)
  {
    if (localAiSlots.Length != LocalAiSlotCount)
    {
      throw new ArgumentException(
        $"NPC local AI state must contain exactly {LocalAiSlotCount} slots.",
        nameof(localAiSlots));
    }

    LocalAiSlots = localAiSlots.ToArray();
  }

  public float[] LocalAiSlots { get; }
}
