using System.Collections.Immutable;

namespace Terraria.SpatialSimulation;

// status: proposed
// resultId: SPATIAL.RESULT.CONTACT_SNAPSHOT
// crossSubsystemOwner: integration-review
public sealed class SpatialContactSnapshot
{
  internal SpatialContactSnapshot(
    long revision,
    ImmutableArray<SpatialEntityContact> entityContacts,
    ImmutableArray<SpatialTileContact> tileContacts,
    bool hasUnsupportedSlopeFacts)
  {
    Revision = revision;
    EntityContacts = entityContacts;
    TileContacts = tileContacts;
    HasUnsupportedSlopeFacts = hasUnsupportedSlopeFacts;
  }

  public long Revision { get; }

  public ImmutableArray<SpatialEntityContact> EntityContacts { get; }

  public ImmutableArray<SpatialTileContact> TileContacts { get; }

  public bool HasUnsupportedSlopeFacts { get; }

  public bool HasEntityContact => EntityContacts.Length != 0;

  public bool HasTileContact => TileContacts.Length != 0;

  public bool IsWet => HasContactFlag(static contact => contact.IsWet);

  public bool IsLavaWet => HasContactFlag(static contact => contact.IsLavaWet);

  public bool IsHoneyWet => HasContactFlag(static contact => contact.IsHoneyWet);

  public bool IsShimmerWet => HasContactFlag(static contact => contact.IsShimmerWet);

  private bool HasContactFlag(
    System.Func<SpatialTileContact, bool> predicate)
  {
    for (int index = 0; index < TileContacts.Length; index++)
    {
      if (predicate(TileContacts[index]))
      {
        return true;
      }
    }

    return false;
  }
}
