using Terraria.Relationships;
using Terraria.WorldInteraction.Tiles;

namespace Terraria.Teleportation;

/// <summary>
/// 保存传送门端点的角度、方向、形式和支撑方块。
/// </summary>
/// <remarks>
/// <para>拆分来源：由 PortalHelper 的端点几何、形式和支撑检查流程重组。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.GameContent/PortalHelper.cs。</para>
/// <para>重组说明：端点实体关系与支撑方块按独立传送门模型重组。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P01-liquid-wiring-spatial-death-teleport-component-design.md。
/// </para>
/// <para>依据位置：第 1715 行。</para>
/// </remarks>
public struct PortalEndpointComponent
{
  public float Angle;
  public int Direction;
  public int Form;
  public EntityReference PortalEntity;
  public TileCoordinate SupportTile;

  public EntityReference PortalEntityId
  {
    get => PortalEntity;
    set => PortalEntity = value;
  }

}
