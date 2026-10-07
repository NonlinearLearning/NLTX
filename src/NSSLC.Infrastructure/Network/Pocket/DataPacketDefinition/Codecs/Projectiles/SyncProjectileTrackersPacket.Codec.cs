using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class SyncProjectileTrackersPacket
{
    public static void Write(
        PacketWireWriter writer,
        byte player,
        PacketTrackedProjectileRef piggyBank,
        PacketTrackedProjectileRef voidLens)
    {
        writer.WriteByte(player);
        WriteReference(writer, piggyBank);
        WriteReference(writer, voidLens);
    }

    public static (byte Player, PacketTrackedProjectileRef PiggyBank, PacketTrackedProjectileRef VoidLens) Read(PacketWireReader reader)
    {
        byte player = reader.ReadByte();
        PacketTrackedProjectileRef piggyBank = ReadReference(reader);
        PacketTrackedProjectileRef voidLens = ReadReference(reader);
        return (Player: player, PiggyBank: piggyBank, VoidLens: voidLens);
    }

    private static void WriteReference(PacketWireWriter writer, PacketTrackedProjectileRef reference)
    {
        bool hasProjectile = reference.Owner != -1;
        if (hasProjectile != (reference.Identity.HasValue && reference.Type.HasValue) || (!hasProjectile && (reference.Identity.HasValue || reference.Type.HasValue)))
        {
            throw new PacketWireFormatException("Packet 142 tracker identity and type are present exactly when owner is not -1.");
        }

        writer.WriteInt16(reference.Owner);
        if (hasProjectile)
        {
            writer.WriteInt16(reference.Identity!.Value);
            writer.WriteInt16(reference.Type!.Value);
        }
    }

    private static PacketTrackedProjectileRef ReadReference(PacketWireReader reader)
    {
        short owner = reader.ReadInt16();
        if (owner == -1)
            return new PacketTrackedProjectileRef(owner, null, null);
        return new PacketTrackedProjectileRef(owner, reader.ReadInt16(), reader.ReadInt16());
    }
}
