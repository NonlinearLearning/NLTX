using System;
using Terraria.NonAuthoritative.Persistence;

namespace MinimalImplementation.Server.Persistence;

/// <summary>Result of reading and validating a candidate document, before any world is published.</summary>
public sealed class WorldDocumentLoadResult
{
  private WorldDocumentLoadResult(
    WorldPersistenceDocument? document,
    WorldStorageFailure failure)
  {
    Document = document;
    Failure = failure;
  }

  public WorldPersistenceDocument? Document { get; }

  public WorldStorageFailure Failure { get; }

  public bool IsValidated => Document is not null && Failure.Kind == WorldStorageFailureKind.None;

  public static WorldDocumentLoadResult Validated(WorldPersistenceDocument document)
  {
    ArgumentNullException.ThrowIfNull(document);
    return new WorldDocumentLoadResult(document, WorldStorageFailure.None);
  }

  public static WorldDocumentLoadResult Rejected(WorldStorageFailure failure)
  {
    return new WorldDocumentLoadResult(
      null,
      failure.Kind == WorldStorageFailureKind.None
        ? WorldStorageFailure.Create(WorldStorageFailureKind.InvalidData)
        : failure);
  }
}
