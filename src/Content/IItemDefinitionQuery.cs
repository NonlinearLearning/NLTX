namespace Terraria.Content;

public interface IItemDefinitionQuery
{
  bool TryGet(int typeId, out ItemDefinition definition);
}
