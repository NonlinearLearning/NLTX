namespace Terraria.NonAuthoritative.Platform;

public readonly record struct FilePlatformOperationResult(
  bool Succeeded,
  FilePlatformFailure Failure)
{
  public static FilePlatformOperationResult Success => new(true, FilePlatformFailure.None);

  public static FilePlatformOperationResult Failed(FilePlatformFailure failure)
  {
    return new(false, failure.Kind == FilePlatformFailureKind.None
      ? FilePlatformFailure.Create(FilePlatformFailureKind.IoFailure)
      : failure);
  }
}
