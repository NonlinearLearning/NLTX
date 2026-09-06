namespace Terraria.Content;

public interface IProjectileDefinitionQuery
{
  bool TryGet(int typeId, out ProjectileDefinition definition);
}
