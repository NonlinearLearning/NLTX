namespace Terraria.Fishing;

// status: proposed
// componentId: FISHING-CATCH-DECISION
// designStatus: candidate
// crossSubsystemOwner: integration-review
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
