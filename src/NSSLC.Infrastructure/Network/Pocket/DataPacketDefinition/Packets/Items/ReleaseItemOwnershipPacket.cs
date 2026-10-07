namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class ReleaseItemOwnershipPacket
{
    public short ItemIndex { get; set; }

    public bool ForceAssignToServer { get; set; }
}
