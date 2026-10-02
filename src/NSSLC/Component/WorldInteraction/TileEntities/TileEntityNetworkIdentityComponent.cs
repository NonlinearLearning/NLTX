namespace Terraria.WorldInteraction.TileEntities;

public sealed class TileEntityNetworkIdentityComponent
{
  // TODO [BD-COMP-04]:
  // 替换为连接范围内确定的 NetworkId 类型。
  // 不得与 runtime ID 或 persistent ID 共用字段。
  public int? NetworkId { get; internal set; }
}
