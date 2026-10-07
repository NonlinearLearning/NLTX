using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class SyncItemsWithShimmerPacket
{
    public static void Write(PacketWireWriter writer, PacketItemSyncData item, bool shimmered, float shimmerTime)
    {
        PacketItemSyncCodec.Write(writer, item);
        writer.WriteBoolean(shimmered);
        writer.WriteSingle(shimmerTime);
    }

    public static (PacketItemSyncData Item, bool Shimmered, float ShimmerTime) Read(PacketWireReader reader)
    {
        PacketItemSyncData item = PacketItemSyncCodec.Read(reader);
        bool shimmered = reader.ReadBoolean();
        float shimmerTime = reader.ReadSingle();
        return (Item: item, Shimmered: shimmered, ShimmerTime: shimmerTime);
    }
}
