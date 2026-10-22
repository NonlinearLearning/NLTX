using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class QuickStackChestsResponsePacket
{
    private const int MaximumBodyBytes = ushort.MaxValue - 3;

    public static void Write(PacketWireWriter writer, ushort[] blockedChestIndexes)
    {
        ArgumentNullException.ThrowIfNull(blockedChestIndexes);
        if (blockedChestIndexes.Length > (MaximumBodyBytes - sizeof(int)) / sizeof(ushort))
            throw new PacketWireFormatException("Packet 85 blocked-chest count exceeds the maximum frame size.");

        writer.WriteInt32(blockedChestIndexes.Length);
        foreach (ushort chestIndex in blockedChestIndexes)
            writer.WriteUInt16(chestIndex);
    }

    public static ushort[] Read(PacketWireReader reader)
    {
        int count = reader.ReadInt32();
        if (count < 0 || count > reader.Remaining / sizeof(ushort)
            || reader.Remaining != count * sizeof(ushort))
        {
            throw new PacketWireFormatException("Packet 85 blocked-chest count does not match its bounded body.");
        }

        var blockedChestIndexes = new ushort[count];
        for (int index = 0; index < blockedChestIndexes.Length; index++)
            blockedChestIndexes[index] = reader.ReadUInt16();
        return blockedChestIndexes;
    }
}
