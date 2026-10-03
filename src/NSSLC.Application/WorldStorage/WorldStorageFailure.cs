namespace Terraria.NonAuthoritative.Persistence;

public readonly record struct WorldStorageFailure(
  WorldStorageFailureKind Kind,
  string? Detail)
{
  public static WorldStorageFailure None => new(WorldStorageFailureKind.None, null);

  public static WorldStorageFailure Create(
    WorldStorageFailureKind kind,
    string? detail = null)
  {
    return kind == WorldStorageFailureKind.None
      ? None
      : new WorldStorageFailure(kind, detail);
  }
}
