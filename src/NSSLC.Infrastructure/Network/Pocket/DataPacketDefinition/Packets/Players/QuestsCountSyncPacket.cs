namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class QuestsCountSyncPacket
{
    public byte Player { get; set; }
    public int AnglerQuestsFinished { get; set; }
    public int GolferScore { get; set; }
}
