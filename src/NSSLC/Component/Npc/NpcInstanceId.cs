namespace Terraria.Npc;

public readonly record struct NpcInstanceId(ulong Value)
{
  public bool IsValid => Value != 0;
}
