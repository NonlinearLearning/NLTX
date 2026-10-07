namespace Terraria.Content;

public interface IBuffDefinitionQuery
{
  bool TryGet(int typeId, out BuffDefinition definition);
}
