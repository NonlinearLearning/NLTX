namespace Terraria.Communication;

/// <summary>Current lifecycle state of a host-owned communication session.</summary>
public enum CommunicationSessionStage : byte
{
  Connected = 0,
  Active = 1,
  Closing = 2,
  Closed = 3
}
