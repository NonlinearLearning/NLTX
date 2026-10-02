namespace Terraria.ExternalPlatformBoundaries.RuntimeComposition.Platform;

public enum PlatformExecutionResult
{
  Acquired,
  AlreadyHeld,
  Released,
  NotHeld,
  Unsupported,
  NativeFailure
}
