namespace Terraria.Network;

/// <summary>Current lifecycle state of a host-owned communication session.</summary>
public enum CommunicationSessionStage : byte
{
  Connected = 0,
  AwaitPlayerData = 1,
  AwaitSectionRequest = 2,
  Synchronizing = 3,
  Active = 4,
  Closing = 5,
  Closed = 6
}
