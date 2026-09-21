namespace Terraria.NonAuthoritative.WorldSession;

public enum WorldLoadStatus
{
  Ok,
  Missing,
  Malformed,
  LaterVersion,
  UnknownError,
  CloudUnavailable,
  ValidationFailed
}
