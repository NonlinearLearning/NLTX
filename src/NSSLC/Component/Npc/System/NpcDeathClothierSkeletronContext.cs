using System;

namespace Terraria.Npc;

public readonly record struct NpcDeathClothierSkeletronContext(
  bool IsDay,
  bool HasActiveSkeletron,
  ReadOnlyMemory<NpcDeathPlayerSnapshot> Players)
{
  public const int RequiredPlayerSlots = 255;
}
