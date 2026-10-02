using Terraria.NonAuthoritative.Platform;

namespace Terraria.NonAuthoritative.Persistence;

public readonly record struct WorldSaveEncodeResult(
  bool Succeeded,
  ReadOnlyMemory<byte> Bytes,
  FilePlatformFailure Failure)
{
  public static WorldSaveEncodeResult Success(ReadOnlyMemory<byte> bytes)
  {
    return new(true, bytes.ToArray(), FilePlatformFailure.None);
  }

  public static WorldSaveEncodeResult Failed(FilePlatformFailure failure)
  {
    return new(false, ReadOnlyMemory<byte>.Empty, failure.Kind == FilePlatformFailureKind.None
      ? FilePlatformFailure.Create(FilePlatformFailureKind.InvalidData)
      : failure);
  }
}
