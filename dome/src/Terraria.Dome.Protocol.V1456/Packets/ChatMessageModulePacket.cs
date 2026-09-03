namespace Terraria.Dome.Protocol.V1456.Packets;

public readonly record struct ChatMessageModulePacket(
  string CommandId,
  string Text);
