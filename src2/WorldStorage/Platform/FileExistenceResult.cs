namespace Terraria.NonAuthoritative.Platform;

public readonly record struct FileExistenceResult(
  bool Exists,
  FilePlatformFailure Failure)
{
  public static FileExistenceResult Present => new(true, FilePlatformFailure.None);

  public static FileExistenceResult Absent => new(false, FilePlatformFailure.None);

  public static FileExistenceResult Failed(FilePlatformFailure failure)
  {
    return new(false, failure.Kind == FilePlatformFailureKind.None
      ? FilePlatformFailure.Create(FilePlatformFailureKind.IoFailure)
      : failure);
  }
}
