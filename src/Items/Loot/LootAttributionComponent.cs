using System.Collections.Generic;
using Terraria.Relationships;

namespace Terraria.Items.Loot;

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
