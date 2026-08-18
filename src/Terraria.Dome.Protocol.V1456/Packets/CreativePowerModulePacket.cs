namespace Terraria.Dome.Protocol.V1456.Packets;

public readonly record struct CreativePowerModulePacket(
  ushort PowerId,
  CreativePowerPayloadKind PayloadKind,
  byte? PlayerSlot,
  bool? ToggleState,
  float? SliderValue,
  byte[]? PlayerStateBits);
