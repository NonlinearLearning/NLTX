namespace MinimalImplementation.Server.Protocol;

public sealed record Version4EncodedPacket(byte MessageId, ReadOnlyMemory<byte> Payload);
