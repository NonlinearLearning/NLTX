namespace Terraria.Items.Loot;

/// <summary>
/// 保存掉落表来源及该来源是否已经结算。
/// </summary>
/// <remarks>
/// <para>拆分来源：由 NPC.NPCLoot 的掉落判定与结算流程重组。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/NPC.cs。</para>
/// <para>重组说明：掉落表引用、是否已结算和结算序号是防止重复结算的新增状态。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-1/design/。</para>
/// <para>拆分依据文件：2026-09-05-version4-spawn-lifecycle-and-loot-component-code-draft.md。</para>
/// <para>依据位置：第 80 行。</para>
/// </remarks>
public sealed class LootSourceComponent
{
  public LootSourceComponent(
    int lootTableId = 0,
    LootSourceKind sourceKind = LootSourceKind.Unknown,
    bool hasResolvedLoot = false,
    long resolutionSequence = 0,
    long? resolvedAtTick = null)
  {
    LootTableId = lootTableId;
    SourceKind = sourceKind;
    HasResolvedLoot = hasResolvedLoot;
    ResolutionSequence = resolutionSequence;
    ResolvedAtTick = resolvedAtTick;
  }

  public int LootTableId;
  public LootSourceKind SourceKind;
  public bool HasResolvedLoot;
  public long ResolutionSequence;
  public long? ResolvedAtTick;

  public bool CanResolveLoot => LootTableId > 0 && !HasResolvedLoot;
}
