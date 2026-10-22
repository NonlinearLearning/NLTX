using MinimalImplementation.Contracts;

namespace MinimalImplementation.Combat.Projectiles;

/// <summary>Validated packet-142 projection for one authenticated player session.</summary>
public sealed class ProjectileTrackerSnapshotView
{
  internal ProjectileTrackerSnapshotView(
    byte player,
    SessionEpoch ownerEpoch,
    ProjectileTrackerReference? piggyBank,
    ProjectileTrackerReference? voidLens,
    long revision,
    CommitVersion commitVersion)
  {
    Player = player;
    OwnerEpoch = ownerEpoch;
    PiggyBank = piggyBank;
    VoidLens = voidLens;
    Revision = revision;
    CommitVersion = commitVersion;
  }

  public byte Player { get; }

  public SessionEpoch OwnerEpoch { get; }

  public ProjectileTrackerReference? PiggyBank { get; }

  public ProjectileTrackerReference? VoidLens { get; }

  public long Revision { get; }

  public CommitVersion CommitVersion { get; }
}
