namespace Terraria.Content;

public sealed record TileCollisionDefinition(
  bool Solid,
  bool SolidTop,
  bool Bouncy = false,
  bool NoAttach = false,
  bool NoFail = false,
  bool Platform = false,
  int? AxePowerRequired = null,
  int? PickPowerRequired = null,
  int? HammerPowerRequired = null);
