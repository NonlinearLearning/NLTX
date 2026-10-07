namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class ItemRotationAndAnimationPacket
{
    public byte Player { get; set; }
    public float ItemRotation { get; set; }
    public short ItemAnimation { get; set; }
}
