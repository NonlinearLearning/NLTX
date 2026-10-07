namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public static class PacketFrameLimits
{
    public const int LengthPrefixBytes = sizeof(ushort);
    public const int MessageIdBytes = sizeof(byte);
    public const int MaximumFrameBytes = ushort.MaxValue;
    public const int MaximumPayloadBytes = MaximumFrameBytes - LengthPrefixBytes - MessageIdBytes;
    public static FrameLayout Layout { get; } = new(MaximumFrameBytes, LengthPrefixBytes + MessageIdBytes);
}
