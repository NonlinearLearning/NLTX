using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class LiquidUpdatePacket
{
    public static void Write(
        PacketWireWriter writer,
        short x,
        short y,
        byte liquidAmount,
        byte liquidType)
    {
        writer.WriteInt16(x);
        writer.WriteInt16(y);
        writer.WriteByte(liquidAmount);
        writer.WriteByte(liquidType);
    }

    public static (short X, short Y, byte LiquidAmount, byte LiquidType) Read(PacketWireReader reader)
    {
        short x = reader.ReadInt16();
        short y = reader.ReadInt16();
        byte amount = reader.ReadByte();
        byte liquidType = reader.ReadByte();
        return (X: x, Y: y, LiquidAmount: amount, LiquidType: liquidType);
    }
}
