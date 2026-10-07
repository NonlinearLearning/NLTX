using System.Collections.Generic;
using Terraria.Relationships;

namespace Terraria.Items.Loot;

/// <summary>
/// 保存掉落的击杀者、幸运值归属及可领取对象。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.NPC。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/NPC.cs。</para>
/// <para>主要源成员：playerInteraction（第 5973 行）； lastInteraction（第 5975 行）。</para>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>主要源成员：luck（第 2431 行）。</para>
/// <para>重组说明：击杀者、幸运值归属和可领取者按实体引用重组。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-1/design/。</para>
/// <para>拆分依据文件：2026-09-05-version4-spawn-lifecycle-and-loot-component-design.md。</para>
/// <para>依据位置：第 516 行。</para>
/// </remarks>
public sealed class LootAttributionComponent
{
  private readonly List<EntityReference> _eligibleRecipients;

  public LootAttributionComponent(
    EntityReference killer = default,
    IReadOnlyList<EntityReference>? eligibleRecipients = null,
    EntityReference luckOwner = default,
    WorldPosition sourcePosition = default,
    long attributionRevision = 0)
  {
    Killer = killer;
    _eligibleRecipients = eligibleRecipients is null
      ? []
      : new List<EntityReference>(eligibleRecipients);
    LuckOwner = luckOwner;
    SourcePosition = sourcePosition;
    AttributionRevision = attributionRevision;
  }

  public EntityReference Killer;
  public EntityReference LuckOwner;
  public WorldPosition SourcePosition;
  public long AttributionRevision;

  public IReadOnlyList<EntityReference> EligibleRecipients => _eligibleRecipients;
  public bool HasEligibleRecipients => _eligibleRecipients.Count > 0;
}
