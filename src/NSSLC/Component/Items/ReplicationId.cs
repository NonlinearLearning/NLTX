namespace Terraria.Items;

public readonly record struct ReplicationId(long Value)
{
  public bool IsAssigned => Value != 0;
}
