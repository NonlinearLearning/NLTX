namespace Terraria.WorldInteraction.TileEntities;

public readonly record struct StoredItemState(int Type, byte Prefix, int Stack)
{
  public bool IsEmpty => Type == 0 || Stack <= 0;
}
