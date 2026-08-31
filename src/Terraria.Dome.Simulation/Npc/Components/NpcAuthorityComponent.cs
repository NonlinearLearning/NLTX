using System;

namespace Terraria.Dome.Simulation.Npc.Components;

public readonly record struct NpcAuthorityComponent
{
  public NpcAuthorityComponent(
    int aiStyle,
    bool isImmortal,
    bool alwaysReplicate,
    float takenDamageMultiplier = 1.0f,
    float npcSlotCost = 1.0f,
    bool isTrapImmune = false,
    bool isLavaImmune = false)
  {
    if (!float.IsFinite(takenDamageMultiplier) || takenDamageMultiplier < 1.0f)
    {
      throw new ArgumentOutOfRangeException(nameof(takenDamageMultiplier));
    }

    if (!float.IsFinite(npcSlotCost) || npcSlotCost < 0.0f)
    {
      throw new ArgumentOutOfRangeException(nameof(npcSlotCost));
    }

    AiStyle = aiStyle;
    IsImmortal = isImmortal;
    AlwaysReplicate = alwaysReplicate;
    TakenDamageMultiplier = takenDamageMultiplier;
    NpcSlotCost = npcSlotCost;
    IsTrapImmune = isTrapImmune;
    IsLavaImmune = isLavaImmune;
  }

  public int AiStyle { get; }
  public bool IsImmortal { get; }
  public bool AlwaysReplicate { get; }
  public float TakenDamageMultiplier { get; }
  public float NpcSlotCost { get; }
  public bool IsTrapImmune { get; }
  public bool IsLavaImmune { get; }
}
