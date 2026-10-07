namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class SyncNPCPacket
{
    public short NpcSlot { get; set; }
    public float PositionX { get; set; }
    public float PositionY { get; set; }
    public float VelocityX { get; set; }
    public float VelocityY { get; set; }
    public ushort Target { get; set; }
    public bool DirectionPositive { get; set; }
    public bool DirectionYPositive { get; set; }
    public bool SpriteDirectionPositive { get; set; }
    public bool FullLife { get; set; }
    public bool SpawnedFromStatue { get; set; }
    public bool SpawnNeedsSyncing { get; set; }
    public bool Shimmering { get; set; }
    public float[] Ai { get; set; } = new float[4];
    public short NetId { get; set; }
    public byte? PlayerCount { get; set; }
    public float? Difficulty { get; set; }
    public int? Life { get; set; }
    public byte? EncodedLifeWidth { get; set; }
    public byte? CatchableReleaseOwner { get; set; }
}
