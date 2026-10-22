using System;

namespace Terraria.NonAuthoritative.Persistence;

public sealed class WorldPersistenceDecodeResult
{
  private WorldPersistenceDecodeResult(
    WorldPersistenceDocument? document,
    WorldStorageFailure failure)
  {
    Document = document;
    Failure = failure;
  }

  public WorldPersistenceDocument? Document { get; }

  public WorldStorageFailure Failure { get; }

  public bool Succeeded => Document is not null && Failure.Kind == WorldStorageFailureKind.None;

  public static WorldPersistenceDecodeResult Decoded(WorldPersistenceDocument document)
  {
    ArgumentNullException.ThrowIfNull(document);
    return new WorldPersistenceDecodeResult(document, WorldStorageFailure.None);
  }

  public static WorldPersistenceDecodeResult Failed(WorldStorageFailure failure)
  {
    WorldStorageFailure actualFailure = failure.Kind == WorldStorageFailureKind.None
      ? WorldStorageFailure.Create(WorldStorageFailureKind.InvalidData)
      : failure;
    return new WorldPersistenceDecodeResult(null, actualFailure);
  }
}
