using System;

namespace Terraria.Share.Physics;

/// <summary>Pure spatial calculation output associated with one entity and one world frame.</summary>
public readonly record struct SpatialSimulationResult<TEntity>
    where TEntity : notnull
{
  private SpatialSimulationResult(
      SpatialSimulationStatus status,
      TEntity entity,
      SpatialWorldRevision worldRevision,
      SpatialCommit commit,
      bool jumpConsumed,
      string? detail)
  {
    Status = status;
    Entity = entity;
    WorldRevision = worldRevision;
    Commit = commit;
    JumpConsumed = jumpConsumed;
    Detail = detail;
  }

  public SpatialSimulationStatus Status { get; }

  public TEntity Entity { get; }

  public SpatialWorldRevision WorldRevision { get; }

  public SpatialCommit Commit { get; }

  public bool JumpConsumed { get; }

  public string? Detail { get; }

  public static SpatialSimulationResult<TEntity> Applied(
      TEntity entity,
      SpatialWorldRevision worldRevision,
      SpatialCommit commit,
      bool jumpConsumed)
  {
    return new SpatialSimulationResult<TEntity>(
        SpatialSimulationStatus.Applied,
        entity,
        worldRevision,
        commit,
        jumpConsumed,
        null);
  }

  public static SpatialSimulationResult<TEntity> Rejected(
      TEntity entity,
      SpatialWorldRevision worldRevision,
      SpatialSimulationStatus status,
      string detail)
  {
    if (status is SpatialSimulationStatus.Applied)
    {
      throw new ArgumentOutOfRangeException(nameof(status));
    }

    ArgumentException.ThrowIfNullOrEmpty(detail);
    return new SpatialSimulationResult<TEntity>(
        status,
        entity,
        worldRevision,
        default,
        false,
        detail);
  }
}
