namespace Terraria.NonAuthoritative.Platform;

public enum FilePlatformFailureKind
{
  None,
  Missing,
  ProviderUnavailable,
  ProviderRejected,
  PermissionDenied,
  ReadOnly,
  InvalidPath,
  IoFailure,
  InvalidData,
  ResultUnknown,
  Canceled,
  Unknown
}
