using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class SyncRevengeMarkerPacket
{
    public static void Write(
        PacketWireWriter writer,
        int uniqueId,
        PacketVector2 position,
        int npcNetId,
        float npcHpPercent,
        int npcType,
        int npcAiStyle,
        int coinsValue,
        float baseValue,
        bool spawnedFromStatue)
    {
        writer.WriteInt32(uniqueId);
        WriteVector2(writer, position);
        writer.WriteInt32(npcNetId);
        writer.WriteSingle(npcHpPercent);
        writer.WriteInt32(npcType);
        writer.WriteInt32(npcAiStyle);
        writer.WriteInt32(coinsValue);
        writer.WriteSingle(baseValue);
        writer.WriteBoolean(spawnedFromStatue);
    }

    public static (
        int UniqueId,
        PacketVector2 Position,
        int NpcNetId,
        float NpcHpPercent,
        int NpcType,
        int NpcAiStyle,
        int CoinsValue,
        float BaseValue,
        bool SpawnedFromStatue) Read(PacketWireReader reader)
    {
        int uniqueId = reader.ReadInt32();
        PacketVector2 position = ReadVector2(reader);
        int npcNetId = reader.ReadInt32();
        float hpPercent = reader.ReadSingle();
        int npcType = reader.ReadInt32();
        int aiStyle = reader.ReadInt32();
        int coinsValue = reader.ReadInt32();
        float baseValue = reader.ReadSingle();
        bool statue = reader.ReadBoolean();
        return (UniqueId: uniqueId, Position: position, NpcNetId: npcNetId, NpcHpPercent: hpPercent, NpcType: npcType, NpcAiStyle: aiStyle, CoinsValue: coinsValue, BaseValue: baseValue, SpawnedFromStatue: statue);
    }

    private static void WriteVector2(PacketWireWriter writer, PacketVector2 value)
    {
        writer.WriteSingle(value.X);
        writer.WriteSingle(value.Y);
    }

    private static PacketVector2 ReadVector2(PacketWireReader reader) => new(reader.ReadSingle(), reader.ReadSingle());
}
