using System;

namespace Terraria.Town;

// status: proposed
// evidenceStatus: partial for normalized status and revision semantics
// crossSubsystemOwner: integration-review
public sealed class TownHousingRelationComponent
{
  public TownHousingRelationComponent(
    bool isHomeless,
    TownRoomTilePoint? homeTile,
    bool homelessDespawn,
    int homeSearchTimeout,
    uint assignmentRevision)
  {
    if (homeSearchTimeout < 0)
    {
      throw new ArgumentOutOfRangeException(
        nameof(homeSearchTimeout),
        "Home search timeout cannot be negative.");
    }

    if (isHomeless && homeTile.HasValue)
    {
      throw new ArgumentException(
        "A stable homeless relation cannot also have an assigned home tile.",
        nameof(homeTile));
    }

    IsHomeless = isHomeless;
    HomeTile = homeTile;
    HomelessDespawn = homelessDespawn;
    HomeSearchTimeout = homeSearchTimeout;
    AssignmentRevision = assignmentRevision;
  }

  public bool IsHomeless { get; }

  public TownRoomTilePoint? HomeTile { get; }

  public bool HomelessDespawn { get; }

  public int HomeSearchTimeout { get; }

  public uint AssignmentRevision { get; }

  public bool HasHome => HomeTile.HasValue && !IsHomeless;
}
