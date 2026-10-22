namespace Terraria.NonAuthoritative.Persistence;

public readonly record struct WorldStorageOperationResult(
  bool Succeeded,
  WorldStorageFailure Failure)
{
  public static WorldStorageOperationResult Success => new(true, WorldStorageFailure.None);

  public static WorldStorageOperationResult Failed(WorldStorageFailure failure)
  {
    return new(false, failure.Kind == WorldStorageFailureKind.None
      ? WorldStorageFailure.Create(WorldStorageFailureKind.IoFailure)
      : failure);
  }
}
