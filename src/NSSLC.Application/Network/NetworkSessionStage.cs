namespace Terraria.Network;

[Flags]
public enum NetworkSessionStage : ushort {
  AwaitHello = 1,
  AwaitPassword = 2,
  AwaitPlayerData = 4,
  AwaitSectionRequest = 8,
  Synchronizing = 16,
  Active = 32,
  Closing = 64,
  Closed = 128
}
