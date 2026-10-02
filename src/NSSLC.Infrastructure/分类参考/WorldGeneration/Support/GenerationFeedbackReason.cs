namespace Terraria.WorldGeneration.Support;

public enum GenerationFeedbackReason : byte
{
  None = 0,
  StartedInASolidTile,
  TooCloseToWorldEdge,
  AnyBlockScannedHere,
  RoomTooBig,
  BlockingWall,
  BlockingOpenGate,
  Stinkbug,
  EchoStinkbug,
  MissingAWall,
  UnsafeWall
}
