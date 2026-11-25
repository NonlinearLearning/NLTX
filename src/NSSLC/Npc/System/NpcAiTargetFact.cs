using System.Numerics;

namespace Terraria.Unshared.Npc;

/// <summary>Detached target facts selected by the NPC host adapter.</summary>
public readonly record struct NpcAiTargetFact(
    Vector2 Position,
    bool IsVisible,
    bool IsAlive,
    long Revision)
{
  public bool IsUsable => IsVisible && IsAlive;
}
