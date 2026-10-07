namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class PlayerSpawnPacket
{
    public byte Player { get; set; }
    public short SpawnX { get; set; }
    public short SpawnY { get; set; }
    public int RespawnTimer { get; set; }
    public short PveDeaths { get; set; }
    public short PvpDeaths { get; set; }
    public byte Team { get; set; }
    public byte SpawnContext { get; set; }
}
