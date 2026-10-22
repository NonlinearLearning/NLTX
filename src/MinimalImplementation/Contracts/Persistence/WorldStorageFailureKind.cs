namespace Terraria.NonAuthoritative.Persistence;

public enum WorldStorageFailureKind
{
  None,
  Missing,
  PermissionDenied,
  ReadOnly,
  InvalidPath,
  IoFailure,
  InvalidData,
  Canceled,
  Unknown
}
