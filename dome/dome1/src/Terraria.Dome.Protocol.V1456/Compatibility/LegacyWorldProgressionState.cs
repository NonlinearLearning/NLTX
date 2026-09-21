namespace Terraria.Dome.Protocol.V1456.Compatibility;

public readonly record struct LegacyWorldProgressionState(
  byte EventFlags1,
  byte EventFlags2,
  byte EventFlags3,
  byte EventFlags4,
  byte EventFlags5,
  byte EventFlags6,
  byte EventFlags7,
  byte EventFlags8,
  byte EventFlags9,
  byte EventFlags10,
  byte EventFlags11,
  byte SundialCooldown,
  byte MoondialCooldown,
  sbyte InvasionType,
  ulong LobbyId,
  float IntendedSandstormSeverity);
