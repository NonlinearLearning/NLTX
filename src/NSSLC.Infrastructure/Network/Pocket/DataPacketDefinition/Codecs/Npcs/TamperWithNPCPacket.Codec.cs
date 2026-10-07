using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class TamperWithNPCPacket
{
    public static void Write(
        PacketWireWriter writer,
        ushort npcIndex,
        byte action,
        int? value,
        short? style)
    {
        bool hasActionData = action == 1;
        if (hasActionData != (value.HasValue && style.HasValue))
            throw new PacketWireFormatException("Packet 131 action 1 requires both value and style fields.");
        if (!hasActionData && (value.HasValue || style.HasValue))
            throw new PacketWireFormatException("Packet 131 only carries value and style for action 1.");
        writer.WriteUInt16(npcIndex);
        writer.WriteByte(action);
        if (hasActionData)
        {
            writer.WriteInt32(value!.Value);
            writer.WriteInt16(style!.Value);
        }
    }

    public static (ushort NpcIndex, byte Action, int? Value, short? Style) Read(PacketWireReader reader)
    {
        ushort npcIndex = reader.ReadUInt16();
        byte action = reader.ReadByte();
        int? value = null;
        short? style = null;
        if (action == 1)
        {
            value = reader.ReadInt32();
            style = reader.ReadInt16();
        }

        return (NpcIndex: npcIndex, Action: action, Value: value, Style: style);
    }
}
