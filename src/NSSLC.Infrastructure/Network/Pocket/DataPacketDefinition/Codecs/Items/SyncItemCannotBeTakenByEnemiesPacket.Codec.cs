using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class SyncItemCannotBeTakenByEnemiesPacket
{
    public static void Write(PacketWireWriter writer, PacketItemSyncData item, byte cannotBeTakenTimer)
    {
        PacketItemSyncCodec.Write(writer, item);
        writer.WriteByte(cannotBeTakenTimer);
    }

    public static (PacketItemSyncData Item, byte CannotBeTakenTimer) Read(PacketWireReader reader)
    {
        PacketItemSyncData item = PacketItemSyncCodec.Read(reader);
        byte timer = reader.ReadByte();
        return (Item: item, CannotBeTakenTimer: timer);
    }
}
