namespace Terraria.WorldInteraction.Structures;

public sealed class PylonStructureComponent
{
  // Version4 style -> TeleportPylonType 的具体 NLTX 类型仍未确定。
  public byte PylonKind { get; internal set; }

  public ushort TileType { get; internal set; } = 597;

  public bool RequiresSolidSupport { get; internal set; } = true;
}
