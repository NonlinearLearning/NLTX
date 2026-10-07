namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class PlayerBuffsPacket
{
    public byte Player { get; set; }
    public IReadOnlyList<ushort> BuffTypes { get; set; } = Array.Empty<ushort>();
}
