using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class FoodPlatterTryPlacingPacket
{
    public static void Write(
        PacketWireWriter writer,
        short x,
        short y,
        short itemType,
        byte prefix,
        short stack) => WeaponsRackTryPlacingPacket.Write(writer, x, y, itemType, prefix, stack);
    public static (
        short X,
        short Y,
        short ItemType,
        byte Prefix,
        short Stack) Read(PacketWireReader reader) => WeaponsRackTryPlacingPacket.Read(reader);
}
