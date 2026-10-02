namespace Terraria.WorldSession.Session;

public readonly record struct ActiveWorldMetadataProjection(
  int PersistentWorldId,
  int GameMode,
  string Name,
  string Path);
