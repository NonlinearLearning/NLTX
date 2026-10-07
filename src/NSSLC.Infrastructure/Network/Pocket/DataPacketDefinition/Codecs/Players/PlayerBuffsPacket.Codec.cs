using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class PlayerBuffsPacket
{
    private const int MaximumBuffCount = (PacketFrameLimits.MaximumPayloadBytes - sizeof(byte) - sizeof(ushort)) / sizeof(ushort);
    public static void Write(PacketWireWriter writer, byte player, IReadOnlyList<ushort> buffTypes)
    {
        writer.WriteByte(player);
        WriteBuffTypes(writer, buffTypes);
    }

    public static (byte Player, IReadOnlyList<ushort> BuffTypes) Read(PacketWireReader reader) => (Player: reader.ReadByte(), BuffTypes: ReadBuffTypes(reader));
    public static void WriteBuffTypes(PacketWireWriter writer, IReadOnlyList<ushort> buffTypes)
    {
        if (buffTypes is null || buffTypes.Count > MaximumBuffCount)
            throw new PacketWireFormatException("Packet 50 buff count exceeds the frame capacity.", PacketReadErrorCode.LengthOutOfRange);
        foreach (ushort buffType in buffTypes)
        {
            if (buffType == 0)
                throw new PacketWireFormatException("Packet 50 buff type zero is reserved as the list terminator.");
            writer.WriteUInt16(buffType);
        }

        writer.WriteUInt16(0);
    }

    public static IReadOnlyList<ushort> ReadBuffTypes(PacketWireReader reader)
    {
        var buffs = new List<ushort>();
        while (true)
        {
            ushort type = reader.ReadUInt16();
            if (type == 0)
                break;
            if (buffs.Count == MaximumBuffCount)
                throw new PacketWireFormatException("Packet 50 buff count exceeds the frame capacity.", PacketReadErrorCode.LengthOutOfRange);
            buffs.Add(type);
        }

        return buffs;
    }
}
