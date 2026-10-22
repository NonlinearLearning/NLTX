namespace Terraria.NonAuthoritative.Persistence;

public readonly record struct WorldSaveEncodeResult(
  bool Succeeded,
  ReadOnlyMemory<byte> Bytes,
  WorldStorageFailure Failure)
{
  public static WorldSaveEncodeResult Success(ReadOnlyMemory<byte> bytes)
  {
    return new(true, bytes.ToArray(), WorldStorageFailure.None);
  }

  public static WorldSaveEncodeResult Failed(WorldStorageFailure failure)
  {
    return new(false, ReadOnlyMemory<byte>.Empty, failure.Kind == WorldStorageFailureKind.None
      ? WorldStorageFailure.Create(WorldStorageFailureKind.InvalidData)
      : failure);
  }
}
