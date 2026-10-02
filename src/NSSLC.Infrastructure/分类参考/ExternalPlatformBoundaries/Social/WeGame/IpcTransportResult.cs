namespace Terraria.ExternalPlatformBoundaries.Social.WeGame;

public enum IpcTransportResult
{
  Opened,
  AlreadyOpen,
  Closed,
  AlreadyClosed,
  Buffered,
  NotOpen,
  Submitted,
  Unavailable,
  Broken,
  Canceled,
  Failed,
  OversizedFrame
}
