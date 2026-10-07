using System;
using System.Collections.Generic;
using NSSLC.WorldGeneration;
using NSSLC.WorldGeneration.Geometry;
using Terraria.NonAuthoritative.Persistence;
using Terraria.WorldStorage;

namespace Terraria.NonAuthoritative.WorldStorage;

/// <summary>Projects restored pressure-plate anchors into the legacy helper.</summary>
public static class LegacyWorldPressurePlateProjection
{
  public static WorldStorageOperationResult Publish(LoadedWorldSession session)
  {
    ArgumentNullException.ThrowIfNull(session);
    return Project(session, allowPublished: false);
  }

  internal static WorldStorageOperationResult ReprojectPublished(LoadedWorldSession session)
  {
    ArgumentNullException.ThrowIfNull(session);
    return Project(session, allowPublished: true);
  }

  private static WorldStorageOperationResult Project(
    LoadedWorldSession session,
    bool allowPublished)
  {
    bool eligibleSession = allowPublished
      ? session.IsComplete && session.IsPublished && !session.IsPublicationUncertain &&
        !session.Lifecycle.IsGeneratingOrLoadingWorld &&
        ReferenceEquals(WorldStorageCoordinatorFactory.ActiveGeneratedWorldSession, session)
      : session.IsComplete && session.IsPublicationUncertain && !session.IsPublished &&
        session.Lifecycle.IsGeneratingOrLoadingWorld;
    if (!eligibleSession)
    {
      return Invalid("Pressure plates can only be published during a gated session commit.");
    }

    IReadOnlyList<TileCoordinate> anchors = session.Storage.PressurePlates.Anchors;
    var stagedPressurePlates = new Dictionary<Point, bool[]>(anchors.Count);
    for (int index = 0; index < anchors.Count; index++)
    {
      TileCoordinate anchor = anchors[index];
      var point = new Point(anchor.X, anchor.Y);
      if (!stagedPressurePlates.TryAdd(
            point,
            session.Storage.PressurePlates.CreateLegacyPlayerStateSnapshot(anchor)))
      {
        return Invalid("Loaded pressure-plate anchors are duplicated.");
      }
    }

    lock (PressurePlateHelper.EntityCreationLock)
    {
      PressurePlateHelper.Reset();
      foreach (KeyValuePair<Point, bool[]> pressurePlate in stagedPressurePlates)
      {
        PressurePlateHelper.PressurePlatesPressed.Add(pressurePlate.Key, pressurePlate.Value);
      }

      PressurePlateHelper.NeedsFirstUpdate = true;
    }

    return WorldStorageOperationResult.Success;
  }

  public static WorldStorageOperationResult PublishCommittedPressStates(
    LoadedWorldSession session,
    IReadOnlyCollection<TileCoordinate> changedAnchors)
  {
    ArgumentNullException.ThrowIfNull(session);
    ArgumentNullException.ThrowIfNull(changedAnchors);
    if (!IsActivePublishedSession(session))
    {
      return Invalid("Pressure-plate state requires the active published session.");
    }
    if (changedAnchors.Count == 0)
    {
      return WorldStorageOperationResult.Success;
    }

    var stagedPressurePlates = new Dictionary<Point, bool[]>(changedAnchors.Count);
    var seenAnchors = new HashSet<TileCoordinate>();
    foreach (TileCoordinate anchor in changedAnchors)
    {
      if (!seenAnchors.Add(anchor))
      {
        continue;
      }
      if (!session.Storage.PressurePlates.ContainsAnchor(anchor))
      {
        return Invalid("The changed pressure-plate anchor is not registered.");
      }

      var point = new Point(anchor.X, anchor.Y);
      stagedPressurePlates.Add(
        point,
        session.Storage.PressurePlates.CreateLegacyPlayerStateSnapshot(anchor));
    }

    lock (PressurePlateHelper.EntityCreationLock)
    {
      foreach (Point point in stagedPressurePlates.Keys)
      {
        if (!PressurePlateHelper.PressurePlatesPressed.ContainsKey(point))
        {
          return Invalid("The runtime pressure-plate registry no longer matches the owner.");
        }
      }

      foreach (KeyValuePair<Point, bool[]> pressurePlate in stagedPressurePlates)
      {
        PressurePlateHelper.PressurePlatesPressed[pressurePlate.Key] = pressurePlate.Value;
      }
    }

    return WorldStorageOperationResult.Success;
  }

  public static WorldStorageOperationResult PublishCommittedRegistry(
    LoadedWorldSession session)
  {
    ArgumentNullException.ThrowIfNull(session);
    if (!IsActivePublishedSession(session))
    {
      return Invalid("The pressure-plate registry requires the active published session.");
    }

    IReadOnlyList<TileCoordinate> anchors = session.Storage.PressurePlates.Anchors;
    var stagedPressurePlates = new Dictionary<Point, bool[]>(anchors.Count);
    foreach (TileCoordinate anchor in anchors)
    {
      var point = new Point(anchor.X, anchor.Y);
      if (!stagedPressurePlates.TryAdd(
            point,
            session.Storage.PressurePlates.CreateLegacyPlayerStateSnapshot(anchor)))
      {
        return Invalid("The committed pressure-plate anchors are duplicated.");
      }
    }

    lock (PressurePlateHelper.EntityCreationLock)
    {
      PressurePlateHelper.Reset();
      foreach (KeyValuePair<Point, bool[]> pressurePlate in stagedPressurePlates)
      {
        PressurePlateHelper.PressurePlatesPressed.Add(
          pressurePlate.Key,
          pressurePlate.Value);
      }
      PressurePlateHelper.NeedsFirstUpdate = true;
    }

    return WorldStorageOperationResult.Success;
  }

  private static bool IsActivePublishedSession(LoadedWorldSession session)
  {
    return session.IsComplete && session.IsPublished &&
      !session.IsPublicationUncertain &&
      !session.Lifecycle.IsGeneratingOrLoadingWorld &&
      ReferenceEquals(WorldStorageCoordinatorFactory.ActiveGeneratedWorldSession, session);
  }

  private static WorldStorageOperationResult Invalid(string detail)
  {
    return WorldStorageOperationResult.Failed(
      WorldStorageFailure.Create(WorldStorageFailureKind.InvalidData, detail));
  }
}
