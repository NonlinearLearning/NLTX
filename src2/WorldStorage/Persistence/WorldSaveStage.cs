namespace Terraria.NonAuthoritative.Persistence;

public enum WorldSaveStage
{
  Capture,
  Encode,
  Commit,
  ReadBack,
  Validate,
  Backup,
  Publish
}
