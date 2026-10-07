namespace Terraria.Content;

public interface ITileDefinitionQuery
{
  bool TryGet(int typeId, out TileDefinition definition);
}
