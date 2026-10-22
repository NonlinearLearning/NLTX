using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class ItemOwnerPacket
{
    public static void Write(
        PacketWireWriter writer,
        short itemIndex,
        byte reservedForPlayer,
        int timeToKeepReservation,
        byte grabDelayPlayer,
        int grabDelayTime,
        float positionX,
        float positionY)
    {
        writer.WriteInt16(itemIndex);
        writer.WriteByte(reservedForPlayer);
        WriteTimer(writer, timeToKeepReservation);
        writer.WriteByte(grabDelayPlayer);
        WriteTimer(writer, grabDelayTime);
        writer.WriteSingle(positionX);
        writer.WriteSingle(positionY);
    }

    public static (short ItemIndex, byte ReservedForPlayer, int TimeToKeepReservation,
        byte GrabDelayPlayer, int GrabDelayTime, float PositionX, float PositionY)
        Read(PacketWireReader reader)
    {
        short index = reader.ReadInt16();
        byte owner = reader.ReadByte();
        int keep = ReadTimer(reader);
        byte grabPlayer = reader.ReadByte();
        int grab = ReadTimer(reader);
        float x = reader.ReadSingle();
        float y = reader.ReadSingle();
        if (index is < 0 or >= 400 || !float.IsFinite(x) || !float.IsFinite(y))
        {
            throw new PacketWireFormatException("Invalid item ownership slot or position.");
        }
        return (index, owner, keep, grabPlayer, grab, x, y);
    }

    private static void WriteTimer(PacketWireWriter writer, int value)
    {
        if (value < 0)
        {
            throw new PacketWireFormatException("An ownership timer cannot be negative.");
        }
        uint remaining = (uint)value;
        while (remaining >= 0x80)
        {
            writer.WriteByte((byte)(remaining | 0x80));
            remaining >>= 7;
        }
        writer.WriteByte((byte)remaining);
    }

    private static int ReadTimer(PacketWireReader reader)
    {
        uint value = 0;
        for (int shift = 0; shift <= 28; shift += 7)
        {
            byte part = reader.ReadByte();
            if (shift == 28 && part > 7)
            {
                throw new PacketWireFormatException("An ownership timer overflows Int32.");
            }
            value |= (uint)(part & 0x7f) << shift;
            if ((part & 0x80) == 0)
            {
                return (int)value;
            }
        }
        throw new PacketWireFormatException("An ownership timer is too long.");
    }
}
