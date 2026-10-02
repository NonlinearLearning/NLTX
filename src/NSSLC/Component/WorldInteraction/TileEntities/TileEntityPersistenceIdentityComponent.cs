namespace Terraria.WorldInteraction.TileEntities;

public sealed class TileEntityPersistenceIdentityComponent
{
  // TODO [BD-COMP-04]:
  // 替换为整合裁决后的持久化 ID 类型。
  // 不得直接复用 TileEntityRuntimeIdComponent.Id。
  public int? PersistentId { get; internal set; }
}
