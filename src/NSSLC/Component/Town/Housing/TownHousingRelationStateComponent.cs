using System;

namespace Terraria.Town.Housing;

public sealed class TownHousingRelationStateComponent
{
  public bool IsHomeless { get; private set; }

  public bool HomelessDespawn { get; private set; }

  public int LookForHomeTimeout { get; private set; }

  public TownRoomTilePoint? HomeTile { get; private set; }

  public int HousingCategory { get; private set; }

  public bool OldHomeless { get; private set; }

  public TownRoomTilePoint? OldHomeTile { get; private set; }

  public bool HasHome => HomeTile.HasValue && !IsHomeless;

  public void CommitRelation(
    bool isHomeless,
    bool homelessDespawn,
    int lookForHomeTimeout,
    TownRoomTilePoint? homeTile,
    int housingCategory)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(lookForHomeTimeout);
    if (isHomeless && homeTile.HasValue)
    {
      throw new ArgumentException(
        "A homeless resident cannot have an assigned home tile.",
        nameof(homeTile));
    }

    CaptureCompatibilitySnapshot();
    IsHomeless = isHomeless;
    HomelessDespawn = homelessDespawn;
    LookForHomeTimeout = lookForHomeTimeout;
    HomeTile = homeTile;
    HousingCategory = housingCategory;
  }

  public void Reset()
  {
    IsHomeless = false;
    HomelessDespawn = false;
    LookForHomeTimeout = 0;
    HomeTile = null;
    HousingCategory = 0;
    OldHomeless = false;
    OldHomeTile = null;
  }

  private void CaptureCompatibilitySnapshot()
  {
    OldHomeless = IsHomeless;
    OldHomeTile = HomeTile;
  }
}
