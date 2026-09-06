namespace Terraria.Npc;

public readonly record struct NpcTypeId(int Value)
{
  public bool IsValid => Value > 0;
}
