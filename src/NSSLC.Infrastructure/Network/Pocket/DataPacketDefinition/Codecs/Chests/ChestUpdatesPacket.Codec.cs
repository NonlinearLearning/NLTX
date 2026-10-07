using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class ChestUpdatesPacket
{
    public static void Write(
        PacketWireWriter writer,
        byte action,
        short x,
        short y,
        short objectType,
        short chestIndex)
    {
        writer.WriteByte(action);
        writer.WriteInt16(x);
        writer.WriteInt16(y);
        writer.WriteInt16(objectType);
        writer.WriteInt16(chestIndex);
    }

    public static (
        byte Action,
        short X,
        short Y,
        short ObjectType,
        short ChestIndex) Read(PacketWireReader reader)
    {
        byte action = reader.ReadByte();
        short x = reader.ReadInt16();
        short y = reader.ReadInt16();
        short objectType = reader.ReadInt16();
        short chest = reader.ReadInt16();
        return (Action: action, X: x, Y: y, ObjectType: objectType, ChestIndex: chest);
    }
}
