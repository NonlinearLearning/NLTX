namespace Terraria.NonAuthoritative.Persistence;

public readonly record struct WorldStorageReadResult(
  bool Succeeded,
  byte[]? Data,
  WorldStorageFailure Failure)
{
  public static WorldStorageReadResult FromBytes(byte[] data)
  {
    ArgumentNullException.ThrowIfNull(data);
    return new(true, data, WorldStorageFailure.None);
  }

  public static WorldStorageReadResult Failed(WorldStorageFailure failure)
  {
    return new(false, null, failure.Kind == WorldStorageFailureKind.None
      ? WorldStorageFailure.Create(WorldStorageFailureKind.IoFailure)
      : failure);
  }
}
