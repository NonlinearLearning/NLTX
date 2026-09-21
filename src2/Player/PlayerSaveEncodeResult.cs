using Terraria.NonAuthoritative.Platform;

namespace Terraria.NonAuthoritative.Player;

public readonly record struct PlayerSaveEncodeResult(
  bool Succeeded,
  ReadOnlyMemory<byte> Bytes,
  FilePlatformFailure Failure)
{
  public static PlayerSaveEncodeResult Success(ReadOnlyMemory<byte> bytes)
  {
    return new(true, bytes.ToArray(), FilePlatformFailure.None);
  }

  public static PlayerSaveEncodeResult Failed(FilePlatformFailure failure)
  {
    return new(false, ReadOnlyMemory<byte>.Empty, failure.Kind == FilePlatformFailureKind.None
      ? FilePlatformFailure.Create(FilePlatformFailureKind.InvalidData)
      : failure);
  }
}
