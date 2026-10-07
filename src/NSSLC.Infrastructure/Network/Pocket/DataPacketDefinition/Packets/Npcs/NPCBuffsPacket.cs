namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed record PacketBuffTime(ushort Type, ushort Time);

public sealed partial class NPCBuffsPacket
{
    public short NpcIndex { get; set; }
    public IReadOnlyList<PacketBuffTime> Buffs { get; set; } = Array.Empty<PacketBuffTime>();
}
