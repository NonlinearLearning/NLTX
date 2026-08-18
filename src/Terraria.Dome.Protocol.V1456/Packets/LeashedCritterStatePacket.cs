namespace Terraria.Dome.Protocol.V1456.Packets;

public readonly record struct LeashedCritterStatePacket(
  int? NpcType,
  float? Width,
  float? Height,
  uint PackedPositionOffset,
  bool FacingRight,
  uint RandomState,
  short WaitTime,
  byte State,
  sbyte TargetOffsetX,
  sbyte TargetOffsetY,
  LeashedCritterExtensionKind ExtensionKind,
  byte? ExtensionValue);
