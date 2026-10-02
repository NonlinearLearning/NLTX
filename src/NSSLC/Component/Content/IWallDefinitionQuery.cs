namespace Terraria.Content;

public interface IWallDefinitionQuery
{
  bool TryGet(int typeId, out WallDefinition definition);
}
