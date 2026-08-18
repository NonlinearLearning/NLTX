namespace Terraria.Dome.Protocol.V1456.Session;

public enum TerrariaSessionState
{
  Connected,
  UserSlotAssigned,
  PlayerProfileReceived,
  WorldDataRequested,
  TileDataRequested,
  AwaitingPlayerSpawn,
  Active,
  Closed
}
