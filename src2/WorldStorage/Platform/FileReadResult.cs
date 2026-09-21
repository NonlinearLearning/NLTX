namespace Terraria.NonAuthoritative.Platform;

public readonly record struct FileReadResult(
  bool Succeeded,
  byte[]? Data,
  FilePlatformFailure Failure)
{
  public static FileReadResult FromBytes(byte[] data)
  {
    ArgumentNullException.ThrowIfNull(data);
    return new(true, data, FilePlatformFailure.None);
  }

  public static FileReadResult Failed(FilePlatformFailure failure)
  {
    return new(false, null, failure.Kind == FilePlatformFailureKind.None
      ? FilePlatformFailure.Create(FilePlatformFailureKind.IoFailure)
      : failure);
  }
}
