using Terraria.NonAuthoritative.Platform;

namespace Terraria.NonAuthoritative.Persistence;

public readonly record struct PreferencesSaveResult(
  PreferencesSaveStatus Status,
  FilePlatformFailure Failure)
{
  public bool Succeeded => Status == PreferencesSaveStatus.Saved;

  public static PreferencesSaveResult Saved => new(PreferencesSaveStatus.Saved, FilePlatformFailure.None);

  public static PreferencesSaveResult SkippedMissingFile =>
    new(PreferencesSaveStatus.SkippedMissingFile, FilePlatformFailure.Create(FilePlatformFailureKind.Missing));

  public static PreferencesSaveResult Failed(FilePlatformFailure failure)
  {
    return new(
      PreferencesSaveStatus.Failed,
      failure.Kind == FilePlatformFailureKind.None
        ? FilePlatformFailure.Create(FilePlatformFailureKind.IoFailure)
        : failure);
  }
}
