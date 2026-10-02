namespace Terraria.Npc;

public readonly record struct NpcSlot(int Value)
{
  public bool IsAssigned => Value >= 0;
}
