using System;
using System.Collections.Generic;

namespace Terraria.WorldStorage;

public sealed class LiquidChangePublicationProjection
{
  private readonly LiquidReplicationDirtySet _dirtySet;
  private long _publishingRevision;

  public LiquidChangePublicationProjection(
    LiquidReplicationDirtySet dirtySet)
  {
    ArgumentNullException.ThrowIfNull(dirtySet);
    _dirtySet = dirtySet;
    _publishingRevision = dirtySet.PublishingCoordinates.Count == 0
      ? 0
      : GetNextRevision(dirtySet.LastPublishedRevision);
  }

  public int PendingCount => _dirtySet.PendingCoordinates.Count;

  public int PublishingCount => _dirtySet.PublishingCoordinates.Count;

  public long LastPublishedRevision => _dirtySet.LastPublishedRevision;

  public bool RecordCommittedChange(TileCoordinate coordinate)
  {
    return _dirtySet.PendingCoordinates.Add(coordinate);
  }

  public LiquidPublicationProjectionResult BeginPublication(
    bool publicationEnabled)
  {
    if (!publicationEnabled)
    {
      return LiquidPublicationProjectionResult.Suppressed(
        _dirtySet.LastPublishedRevision);
    }

    if (_dirtySet.PublishingCoordinates.Count != 0)
    {
      return LiquidPublicationProjectionResult.InFlight(
        CreateSnapshot(_publishingRevision, _dirtySet.PublishingCoordinates));
    }

    if (_dirtySet.PendingCoordinates.Count == 0)
    {
      return LiquidPublicationProjectionResult.Empty(
        _dirtySet.LastPublishedRevision);
    }

    if (_dirtySet.LastPublishedRevision == long.MaxValue)
    {
      return LiquidPublicationProjectionResult.Rejected(
        "revision-exhausted");
    }

    HashSet<TileCoordinate> previousPublishing =
      _dirtySet.PublishingCoordinates;
    HashSet<TileCoordinate> nextPublishing = _dirtySet.PendingCoordinates;
    previousPublishing.Clear();
    _dirtySet.PendingCoordinates = previousPublishing;
    _dirtySet.PublishingCoordinates = nextPublishing;
    _publishingRevision = _dirtySet.LastPublishedRevision + 1;

    return LiquidPublicationProjectionResult.Ready(
      CreateSnapshot(_publishingRevision, nextPublishing));
  }

  public LiquidPublicationProjectionResult AcknowledgePublication(
    long revision)
  {
    if (_dirtySet.PublishingCoordinates.Count == 0)
    {
      return LiquidPublicationProjectionResult.Rejected(
        "no-publishing-batch");
    }

    if (revision != _publishingRevision)
    {
      return LiquidPublicationProjectionResult.Rejected(
        "stale-revision");
    }

    _dirtySet.PublishingCoordinates.Clear();
    _dirtySet.LastPublishedRevision = revision;
    _publishingRevision = 0;
    return LiquidPublicationProjectionResult.Acknowledged(revision);
  }

  public LiquidPublicationProjectionResult GetPublishingSnapshot()
  {
    if (_dirtySet.PublishingCoordinates.Count == 0)
    {
      return LiquidPublicationProjectionResult.Empty(
        _dirtySet.LastPublishedRevision);
    }

    return LiquidPublicationProjectionResult.InFlight(
      CreateSnapshot(_publishingRevision, _dirtySet.PublishingCoordinates));
  }

  public bool Reset()
  {
    bool changed = _dirtySet.PendingCoordinates.Count != 0 ||
      _dirtySet.PublishingCoordinates.Count != 0 ||
      _dirtySet.LastPublishedRevision != 0;
    _dirtySet.PendingCoordinates.Clear();
    _dirtySet.PublishingCoordinates.Clear();
    _dirtySet.LastPublishedRevision = 0;
    _publishingRevision = 0;
    return changed;
  }

  private static LiquidChangePublicationSnapshot CreateSnapshot(
    long revision,
    IReadOnlyCollection<TileCoordinate> coordinates)
  {
    List<TileCoordinate> ordered = new(coordinates);
    ordered.Sort(static (first, second) =>
    {
      int xComparison = first.X.CompareTo(second.X);
      return xComparison != 0
        ? xComparison
        : first.Y.CompareTo(second.Y);
    });
    return new LiquidChangePublicationSnapshot(revision, ordered);
  }

  private static long GetNextRevision(long revision)
  {
    if (revision == long.MaxValue)
    {
      throw new ArgumentOutOfRangeException(nameof(revision));
    }

    return revision + 1;
  }
}
