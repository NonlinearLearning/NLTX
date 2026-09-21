namespace Terraria.Dome.Protocol.V1456.Packets;

public readonly record struct LeashedEntityModulePacket(
  LeashedEntityMessageType MessageType,
  int Slot,
  int? EntityType,
  short? AnchorX,
  short? AnchorY,
  LeashedKiteStatePacket? KiteState,
  LeashedCritterStatePacket? CritterState);
