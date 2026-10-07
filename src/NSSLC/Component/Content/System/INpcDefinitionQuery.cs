namespace Terraria.Content;

public interface INpcDefinitionQuery
{
  bool TryGetByNetId(int netId, out NpcDefinition definition);

  bool TryGetByTypeId(int typeId, out NpcDefinition definition);
}
