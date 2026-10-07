using Terraria.Relationships;

namespace Terraria.Teleportation;

/// <summary>
/// 保存传送门分组及两个端点的配对关系。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.GameContent.PortalHelper。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.GameContent/PortalHelper.cs。</para>
/// <para>主要源成员：FoundPortals（第 12 行）。</para>
/// <para>重组说明：原配对缓存改为分组和对端实体关系。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-1/design/。</para>
/// <para>
/// 拆分依据文件：2026-09-05-version4-teleportation-and-traversal-code-component-draft.md。
/// </para>
/// <para>依据位置：第 256 行。</para>
/// </remarks>
public struct PortalLinkStateComponent
{
  public int PortalGroup;
  public EntityReference? PeerEndpoint;

  public bool IsPairComplete => PeerEndpoint.HasValue;
}
