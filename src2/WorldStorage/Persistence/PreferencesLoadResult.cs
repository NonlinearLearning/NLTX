using Terraria.NonAuthoritative.Platform;

namespace Terraria.NonAuthoritative.Persistence;

public readonly record struct PreferencesLoadResult(
  bool Loaded,
  bool Missing,
  FilePlatformFailure Failure)
{
  public static PreferencesLoadResult Success => new(true, false, FilePlatformFailure.None);

  public static PreferencesLoadResult MissingFile => new(false, true, FilePlatformFailure.None);

  public static PreferencesLoadResult Failed(FilePlatformFailure failure)
  {
    return new(false, false, failure.Kind == FilePlatformFailureKind.None
      ? FilePlatformFailure.Create(FilePlatformFailureKind.InvalidData)
      : failure);
  }
}
