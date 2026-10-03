namespace Terraria.Network;

public sealed record PacketWriteReceipt(
    ConnectionIdentity Connection, long Sequence, byte MessageId, int FrameBytes);
