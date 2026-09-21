namespace Terraria.NonAuthoritative.Platform;

public readonly record struct FilePlatformFailure(
  FilePlatformFailureKind Kind,
  string? Detail)
{
  public static FilePlatformFailure None => new(FilePlatformFailureKind.None, null);

  public static FilePlatformFailure Create(FilePlatformFailureKind kind, string? detail = null)
  {
    return kind == FilePlatformFailureKind.None
      ? None
      : new FilePlatformFailure(kind, detail);
  }
}
