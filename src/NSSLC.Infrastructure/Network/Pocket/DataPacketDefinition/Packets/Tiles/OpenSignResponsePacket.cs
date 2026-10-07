namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class OpenSignResponsePacket
{
    public short SignIndex { get; set; }
    public short X { get; set; }
    public short Y { get; set; }
    public string Text { get; set; } = string.Empty;
    public byte Player { get; set; }
    public byte Flags { get; set; }
}
