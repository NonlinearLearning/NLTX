namespace Terraria.Npc;

public readonly record struct NpcNetId(int Value)
{
  public bool IsVariant => Value < 0;
}
