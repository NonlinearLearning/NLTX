namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

public enum PacketReadStatus : byte
{
    Success,
    Truncated,
    InvalidData
}

public enum PacketReadErrorCode : byte
{
    Truncated,
    InvalidLengthPrefix,
    LengthOutOfRange,
    TrailingBytes,
    InvalidCodecData
}

// Offset is relative to the supplied packet body, including earlier committed reads.
public sealed record PacketReadError(
    PacketReadStatus Status,
    PacketReadErrorCode Code,
    int Offset,
    string? Member,
    string Message);

public sealed class PacketReadResult<TPacket>
{
    private PacketReadResult(TPacket? packet, int consumed, PacketReadError? error)
    {
        Packet = packet;
        Consumed = consumed;
        Error = error;
    }

    public PacketReadStatus Status => Error?.Status ?? PacketReadStatus.Success;
    public bool Success => Status == PacketReadStatus.Success;
    // For value-type packets, Packet is default on failure; inspect Success before using it.
    public TPacket? Packet { get; }
    public int Consumed { get; }
    public PacketReadError? Error { get; }

    public static PacketReadResult<TPacket> Succeeded(TPacket packet, int consumed)
    {
        ArgumentNullException.ThrowIfNull(packet);
        ArgumentOutOfRangeException.ThrowIfNegative(consumed);
        return new(packet, consumed, null);
    }

    public static PacketReadResult<TPacket> Failed(PacketReadError error)
    {
        ArgumentNullException.ThrowIfNull(error);
        if (error.Status == PacketReadStatus.Success)
            throw new ArgumentException("A failed read must have a failure status.", nameof(error));
        return new(default, 0, error);
    }
}
