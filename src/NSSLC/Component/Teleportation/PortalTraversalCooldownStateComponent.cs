using Terraria.Relationships;

namespace Terraria.Teleportation;

/// <summary>
/// 保存主体的传送门穿越冷却、最近端点和冷却分组。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.GameContent.PortalHelper。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.GameContent/PortalHelper.cs。</para>
/// <para>主要源成员：PortalCooldownForPlayers（第 14 行）； PortalCooldownForNPCs（第 16 行）。</para>
/// <para>重组说明：原按槽位保存的冷却数组按旅行主体重组；最近端点和分组是新增关系。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-1/design/。</para>
/// <para>
/// 拆分依据文件：2026-09-05-version4-teleportation-and-traversal-code-component-draft.md。
/// </para>
/// <para>依据位置：第 375 行。</para>
/// </remarks>
public struct PortalTraversalCooldownStateComponent
{
  public bool IsCoolingDown => RemainingTicks > 0;
  public int RemainingTicks;
  public EntityReference? LastPortal;
  public PortalSubjectKind SubjectKind;
  public int? CooldownGroup;
}
