using MinimalImplementation.Contracts;

namespace MinimalImplementation.Server.World;

/// <summary>Projects owner-captured liquid dirty values onto section transfer barriers.</summary>
public sealed class WorldLiquidSnapshotProjection
{
  private readonly WorldSectionTransferCoordinator _sections;

  public WorldLiquidSnapshotProjection(WorldSectionTransferCoordinator sections)
  {
    ArgumentNullException.ThrowIfNull(sections);
    _sections = sections;
  }

  /// <summary>
  /// Groups dirty tiles by their owning section and queues one latest batch version. The owner
  /// dirty batch is not acknowledged here; the caller acknowledges only after the transport
  /// result has been applied, so an unsubmitted delta remains retryable.
  /// </summary>
  public WorldLiquidProjectionResult QueueDirtyBatch(
    WorldSectionTransferToken token,
    WorldLiquidDirtyBatch batch)
  {
    ArgumentNullException.ThrowIfNull(batch);
    if (!token.IsValid || batch.WorldEpoch != token.WorldEpoch || !batch.Version.IsValid)
    {
      return WorldLiquidProjectionResult.Rejected(WorldSectionTransferStatus.StaleWorld);
    }

    if (!_sections.TryGetWorldGeometry(token, out WorldGeometry geometry))
    {
      return WorldLiquidProjectionResult.Rejected(WorldSectionTransferStatus.InvalidSession);
    }

    var sections = new HashSet<WorldSectionCoordinate>();
    foreach (LiquidDirtyValue value in batch.Values)
    {
      if (!geometry.Contains(value.Coordinate))
      {
        return WorldLiquidProjectionResult.Rejected(WorldSectionTransferStatus.InvalidSection);
      }

      sections.Add(new WorldSectionCoordinate(
        value.Coordinate.X / WorldGeometry.SectionWidth,
        value.Coordinate.Y / WorldGeometry.SectionHeight));
    }

    if (sections.Count == 0)
    {
      return WorldLiquidProjectionResult.Empty(batch.Version);
    }

    WorldLiquidDeltaQueueResult queued = _sections.QueueLiquidDelta(
      token,
      sections,
      batch.Version);
    return queued.Status == WorldSectionTransferStatus.Queued
      ? WorldLiquidProjectionResult.Success(queued.Version!.Value, queued.Sections)
      : WorldLiquidProjectionResult.Rejected(queued.Status);
  }
}

/// <summary>Result of mapping a liquid dirty batch to eligible section deltas.</summary>
public sealed class WorldLiquidProjectionResult
{
  private WorldLiquidProjectionResult(
    WorldSectionTransferStatus status,
    CommitVersion? version,
    IReadOnlyCollection<WorldSectionCoordinate> sections)
  {
    Status = status;
    Version = version;
    Sections = Array.AsReadOnly(sections
      .OrderBy(section => section.Y)
      .ThenBy(section => section.X)
      .ToArray());
  }

  public WorldSectionTransferStatus Status { get; }

  public CommitVersion? Version { get; }

  public IReadOnlyList<WorldSectionCoordinate> Sections { get; }

  internal static WorldLiquidProjectionResult Success(
    CommitVersion version,
    IReadOnlyCollection<WorldSectionCoordinate> sections)
  {
    return new WorldLiquidProjectionResult(
      WorldSectionTransferStatus.Queued,
      version,
      sections);
  }

  internal static WorldLiquidProjectionResult Empty(CommitVersion version)
  {
    return new WorldLiquidProjectionResult(
      WorldSectionTransferStatus.Accepted,
      version,
      Array.Empty<WorldSectionCoordinate>());
  }

  internal static WorldLiquidProjectionResult Rejected(WorldSectionTransferStatus status)
  {
    return new WorldLiquidProjectionResult(status, null, Array.Empty<WorldSectionCoordinate>());
  }
}
