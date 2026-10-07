using System;
using System.Collections.Generic;
using Terraria.Items;
using Terraria.Relationships;

namespace Terraria.Items.Loot;

// status: proposed
public sealed class LootAttributionState
{
  private readonly HashSet<EntityReference> _eligibleRecipients;

  public LootAttributionState(
    EntityReference killer = default,
    IReadOnlyCollection<EntityReference>? eligibleRecipients = null,
    EntityReference luckOwner = default,
    WorldPosition sourcePosition = default,
    long attributionRevision = 0)
  {
    if (attributionRevision < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(attributionRevision));
    }

    _eligibleRecipients = eligibleRecipients is null
      ? new HashSet<EntityReference>()
      : new HashSet<EntityReference>(eligibleRecipients);

    if (_eligibleRecipients.Contains(EntityReference.None))
    {
      throw new ArgumentException(
        "Eligible recipients cannot contain EntityReference.None.",
        nameof(eligibleRecipients));
    }

    Killer = killer;
    LuckOwner = luckOwner;
    SourcePosition = sourcePosition;
    AttributionRevision = attributionRevision;
  }

  public EntityReference Killer { get; }

  public IReadOnlySet<EntityReference> EligibleRecipients =>
    _eligibleRecipients;

  public EntityReference LuckOwner { get; }

  public WorldPosition SourcePosition { get; }

  public long AttributionRevision { get; }

  public bool HasEligibleRecipients =>
    _eligibleRecipients.Count > 0;
}
