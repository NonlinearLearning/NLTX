namespace Terraria.Fishing;

// status: proposed
// componentId: FISHING-CATCH-DECISION
// designStatus: candidate
// crossSubsystemOwner: integration-review
/// <summary>
/// 保存一次钓鱼判定选出的物品或 NPC 及数量。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.DataStructures.FishingAttempt。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.DataStructures/FishingAttempt.cs。</para>
/// <para>主要源成员：questFish（第 45 行）。</para>
/// <para>重组说明：DecisionRevision 用于拆分后的版本或实例生命周期管理。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-1/design/。</para>
/// <para>
/// 拆分依据文件：2026-09-05-version4-fishing-and-catch-simulation-component-code-draft.md。
/// </para>
/// <para>依据位置：第 432 行。</para>
/// </remarks>
public struct FishingCatchDecisionComponent
{
  public FishingCatchDecisionComponent(
    FishingOutcomeKind outcomeKind,
    int? itemTypeId,
    int? npcTypeId,
    int stackQuantity,
    bool isQuestFish,
    uint decisionRevision)
  {
    OutcomeKind = outcomeKind;
    ItemTypeId = itemTypeId;
    NpcTypeId = npcTypeId;
    StackQuantity = stackQuantity;
    IsQuestFish = isQuestFish;
    DecisionRevision = decisionRevision;
    RngAuditValue = null;
  }

  public FishingOutcomeKind OutcomeKind;

  // Content definition references, not Item/NPC entity instances.
  public int? ItemTypeId;
  public int? NpcTypeId;

  public int StackQuantity;
  public bool IsQuestFish;

  // Version4 has no explicit attempt-level decision revision.
  public uint DecisionRevision;

  // Opaque audit value; it must not be used to reroll the decision.
  public FishingRngAuditValue? RngAuditValue;

  public bool HasItemResult => ItemTypeId.HasValue;

  public bool HasNpcResult => NpcTypeId.HasValue;
}
