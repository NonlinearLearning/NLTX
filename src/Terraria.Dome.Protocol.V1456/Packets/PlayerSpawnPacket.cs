namespace Terraria.Dome.Protocol.V1456.Packets;

public readonly record struct PlayerSpawnPacket(
  byte PlayerSlot,
  short SpawnX,
  short SpawnY,
  int RespawnTimer,
  short PlayerVersusEnvironmentDeaths,
  short PlayerVersusPlayerDeaths,
  byte Team,
  byte SpawnContext);
