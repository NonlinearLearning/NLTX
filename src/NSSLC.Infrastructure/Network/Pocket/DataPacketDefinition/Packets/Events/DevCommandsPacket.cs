namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class DevCommandsPacket
{
    public string Command { get; set; } = string.Empty;
    public int IntegerArgument { get; set; }
    public float Value { get; set; }
    public float TrailingValue { get; set; }
}
