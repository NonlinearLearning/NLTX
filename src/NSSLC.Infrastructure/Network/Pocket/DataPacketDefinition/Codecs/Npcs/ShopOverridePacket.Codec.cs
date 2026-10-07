using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class ShopOverridePacket
{
    public static void Write(
        PacketWireWriter writer,
        byte player,
        short npcType,
        float priceAdjustment,
        byte shopId,
        int specialCurrency,
        byte shopType)
    {
        writer.WriteByte(player);
        writer.WriteInt16(npcType);
        writer.WriteSingle(priceAdjustment);
        writer.WriteByte(shopId);
        writer.WriteInt32(specialCurrency);
        writer.WriteByte(shopType);
    }

    public static (
        byte Player,
        short NpcType,
        float PriceAdjustment,
        byte ShopId,
        int SpecialCurrency,
        byte ShopType) Read(PacketWireReader reader) => (Player: reader.ReadByte(), NpcType: reader.ReadInt16(), PriceAdjustment: reader.ReadSingle(), ShopId: reader.ReadByte(), SpecialCurrency: reader.ReadInt32(), ShopType: reader.ReadByte());
}
